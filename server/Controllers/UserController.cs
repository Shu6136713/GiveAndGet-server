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
        private readonly IContext context;
        public static string _directory = Path.Combine(Environment.CurrentDirectory, "Images");

        public UserController(IService<UserDto> userService, ITalentUserExtensionService talentUserService, IContext context)
        {
            _userService = userService;
            _talentUserService = talentUserService;
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
                    Console.WriteLine(talentList[0].UserId);
                    _talentUserService.AddTalentsForUser(talentList);
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
        [Authorize]
        [HttpPut("{id}")]
        public UserDto Put(int id, [FromForm] UserDto updateUser, [FromForm] string talents)
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdFromToken != id.ToString())
            {
                throw new Exception("You cannot update user who is not yourself.");
            }

            if (!string.IsNullOrEmpty(updateUser.HashPwd))
            {
                if (!CheckIfValidatePwd(updateUser.HashPwd))
                    throw new Exception("Password must contain upper and lower case letters, numbers, and special characters.");
                string hashPwd = PasswordManagerService.HashPassword(updateUser.HashPwd);
                updateUser.HashPwd = hashPwd;
            }

            if (updateUser.File != null)
            {
                var filePath = Path.Combine(_directory, updateUser.File.FileName);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    updateUser.File.CopyTo(fs);
                }
                updateUser.Profile = updateUser.File.FileName;
            }
            UserDto updatedUser = _userService.Update(id, updateUser);

            if (!string.IsNullOrEmpty(talents) && talents != "[]" && talents!=null)
            {
                List<dynamic> talentList = JsonConvert.DeserializeObject<List<dynamic>>(talents);
                var talentsToRemove = talentList.Where(t => (bool)(t.Remove ?? false)).ToList();
                foreach (var t in talentsToRemove)
                {
                    TalentUserDto talent = _talentUserService.GetTalentsByUserId(updatedUser.Id)
                         .Where(x => x.TalentId == (int)t.TalentId)
                         .FirstOrDefault();
                    if (talent != null)
                    {
                        _talentUserService.Delete(talent.UserId,talent.TalentId);
                    }
                }

                // Convert talents that are not marked for removal to TalentUserDto
                List<TalentUserDto> talentsToAdd = talentList
                    .Where(t => !(bool)(t.Remove ?? false))
                    .Select(t => new TalentUserDto
                    {
                        UserId = updatedUser.Id
                    })
                    .ToList();
                _talentUserService.AddTalentsForUser(talentsToAdd);
            }


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