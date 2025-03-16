using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Repositories.Entity;
using Repositories.Interfaces;
using Services.Dtos;
using Services.Interfaces;
using Services.Services;
using System;
using System.IO;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IService<UserDto> _userService;
        private readonly ITalentUserExtensionService _talentUserService;
        private readonly ITalentExtensionService _talentService;
        private readonly IExchangeExtensionService _exchangeService;
        private readonly IContext context;
        public static string _directory = Path.Combine(Environment.CurrentDirectory, "Images");
        

        public UserController(IService<UserDto> userService, 
            ITalentUserExtensionService talentUserService,
            IExchangeExtensionService exchangeService,
            ITalentExtensionService talentExtensionService,
            IContext context)
        {
            _userService = userService;
            _talentUserService = talentUserService;
            _exchangeService = exchangeService;
            _talentService = talentExtensionService;
            this.context = context;
        }

        // GET: api/<UserController>
        [HttpGet]
        public List<UserDto> Get()
        {
            return _userService.GetAll();
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public UserDto Get(int id)
        {
            return _userService.Get(id);
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdFromToken))
            {
                return Unauthorized("Token is missing or invalid");
            }

            var user = _userService.Get(int.Parse(userIdFromToken));
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!string.IsNullOrEmpty(user.Profile))
            {
                user.ProfileImageUrl = $"{Request.Scheme}://{Request.Host}/api/User/profile-image/{user.Id}";
            }

            return Ok(user);
        }

        // POST api/<UserController>
        [HttpPost]
        public IActionResult Post([FromForm] UserDto user, [FromForm] string talents)
        {
            try
            {
                Console.WriteLine($"User: {JsonConvert.SerializeObject(user)}, Talents: {talents}");

                string pwd = user.HashPwd;

                // בדיקה אם הסיסמא תקינה
                if (!CheckIfValidatePwd(pwd))
                    return BadRequest("Password must contain upper and lower case letters, numbers, and special characters.");

                // חישוב סיסמא מוצפנת
                string hashPwd = PasswordManagerService.HashPassword(pwd);
                user.HashPwd = hashPwd;

                // טיפול בתמונה של פרופיל
                if (user.File != null)
                {
                    if (!Directory.Exists(_directory))
                    {
                        Directory.CreateDirectory(_directory);
                    }
                    var filePath = Path.Combine(_directory, user.File.FileName);
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        user.File.CopyTo(fs);
                    }
                    user.Profile = user.File.FileName;
                }

                // הוספת משתמש
                UserDto newUser = _userService.AddItem(user);


                // הוספת כישרונות למשתמש אם יש
                if (talents != "[]")
                {
                    List<TalentUserDto> talentList = JsonConvert.DeserializeObject<List<TalentUserDto>>(talents);
                    foreach (TalentUserDto t in talentList)
                    {
                        t.UserId = newUser.Id;
                    }
                    Console.WriteLine(talentList.First().UserId);
                    _talentUserService.AddTalentsForUser(talentList);
                    _exchangeService.SearchExchangesForUser(newUser.Id);
                }

                return Ok(newUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private bool CheckIfValidatePwd(string pwd)
        {
            if (pwd.Length < 8 || pwd.Length > 20)
                return false;

            bool foundUp = false, foundLow = false, foundChar = false, foundNum = false;
            for (int i = 0; i < pwd.Length && !(foundChar && foundLow && foundUp && foundNum); i++)
            {
                if (Char.IsUpper(pwd[i]))
                    foundUp = true;
                else if (Char.IsLower(pwd[i]))
                    foundLow = true;
                else if (Char.IsDigit(pwd[i]))
                    foundNum = true;
                else
                    foundChar = true;
            }
            return foundChar && foundLow && foundUp && foundNum;
        }


        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public UserDto Put(int id, [FromForm] UserDto updateUser, [FromForm] string talents)
        {
            Console.WriteLine(talents);

            // אם המשתמש מעדכן סיסמה, מבצעים בדיקות ואימות
            if (!string.IsNullOrEmpty(updateUser.HashPwd))
            {
                if (!CheckIfValidatePwd(updateUser.HashPwd))
                    throw new Exception("Password must contain upper and lower case letters, numbers, and special characters.");

                updateUser.HashPwd = PasswordManagerService.HashPassword(updateUser.HashPwd);
            }

            // אם המשתמש מעלה תמונת פרופיל חדשה, שומרים אותה בשרת
            if (updateUser.File != null)
            {
                var filePath = Path.Combine(_directory, updateUser.File.FileName);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    updateUser.File.CopyTo(fs);
                }
                updateUser.Profile = updateUser.File.FileName;
            }

            // שליפת רשימת הכישרונות הנוכחית של המשתמש מהמערכת
            var currentTalents = _talentUserService.GetTalentsByUserId(id)
                                  .Select(t => t.TalentId).ToList();

            // המרת המחרוזת JSON שהתקבלה לרשימת כישרונות המשתמש
            List<TalentUserDto> talentUserList_new = !string.IsNullOrEmpty(talents) ?
                JsonConvert.DeserializeObject<List<TalentUserDto>>(talents) : new List<TalentUserDto>();

            // רשימת ה- TalentId החדשה לאחר הסינון
            var newTalents = talentUserList_new.Select(t => t.TalentId).Distinct().ToList();

            // זיהוי כישרונות שנמחקו (היו בעבר אך לא ברשימה החדשה)
            var removedTalentIds = currentTalents.Except(newTalents).ToList();

            // זיהוי כישרונות חדשים (נמצאים ברשימה החדשה אך לא היו בעבר)
            var addedTalentIds = newTalents.Except(currentTalents).ToList();

            // זיהוי כישרונות שהיו קיימים אך ייתכן והסטטוס שלהם (IsOffered) השתנה
            var existingTalents = currentTalents.Intersect(newTalents).ToList();
            foreach (var talentId in existingTalents)
            {
                var newTalent = talentUserList_new.FirstOrDefault(t => t.TalentId == talentId);
                if (newTalent != null)
                {
                    _talentUserService.UpdateIsOffered(id, talentId, newTalent.IsOffered);
                }
            }

            // מחיקת הכישרונות שהוסרו מהרשימה
            foreach (var talentId in removedTalentIds)
            {
                _talentUserService.Delete(id, talentId);
            }

            // הוספת כישרונות חדשים (אם יש כאלו)
            if (addedTalentIds.Any())
            {
                var addedTalents = talentUserList_new
                    .Where(t => addedTalentIds.Contains(t.TalentId)) // מסננים רק את הכישרונות החדשים
                    .Select(t => new TalentUserDto { UserId = id, TalentId = t.TalentId, IsOffered = t.IsOffered }) // שומרים את ה- IsOffered
                    .ToList();

                _talentUserService.AddTalentsForUser(addedTalents);
            }

            // עדכון פרטי המשתמש במערכת
            UserDto updatedUser = _userService.Update(id, updateUser);

            // עדכון עסקאות בהתאם לכישרונות שנוספו ונמחקו
            _exchangeService.UpdateUserExchanges(id, removedTalentIds, addedTalentIds);

            return updatedUser;
        }





        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _userService.Delete(id);
        }

        [HttpGet("profile-image/{id}")]
        public IActionResult GetProfileImage(int id)
        {
            UserDto user = _userService.Get(id);
            if (user != null && !string.IsNullOrEmpty(user.Profile))
            {
                var filePath = Path.Combine(_directory, user.Profile);
                if (System.IO.File.Exists(filePath))
                {
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "image/jpeg");
                }
                return NotFound("Profile image not found");
            }
            return NotFound("User or image not found");
        }
    }
}