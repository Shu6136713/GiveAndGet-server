using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mock;
using Repositories.Interfaces;
using Services.Dtos;
using Services.Interfaces;
using Services.Services;
using System;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IService<UserDto> _userService;
        //private readonly PasswordManagerService _passwordManagerService;
        //private readonly GiveAndGetDB _giveAndGetDB;
        private readonly IContext context;
        public static string _directory = Environment.CurrentDirectory + "/Images/";

        public UserController(
            IService<UserDto> userService, IContext context
            //PasswordManagerService passwordManagerService, 
            //GiveAndGetDB giveAndGetDB
            )
        {
            _userService = userService;
            this.context = context;
           // _passwordManagerService = passwordManagerService;
           // _giveAndGetDB = giveAndGetDB;
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

            return Ok(user); // החזר את המידע על המשתמש
        }

        // POST api/<UserController>
        [HttpPost]
        public IActionResult Post([FromForm] UserDto user) //register
        {
           // using (var transaction =  context)
           // {
            try
                {

                string pwd = user.HashPwd;
                if (!CheckIfValidatePwd(pwd))
                    return BadRequest("Password must contain upper and lower case letters, numbers, and special characters.");
                string hashPwd = PasswordManagerService.HashPassword(pwd);
                user.HashPwd = hashPwd;

                //profile img
                if (user.File != null)
                    {
                    if (!Directory.Exists(_directory))
                    {
                        Directory.CreateDirectory(_directory);
                    }
                    var filePath = Path.Combine(_directory, user.File.FileName); //l:/...
                     using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            user.File.CopyTo(fs);
                            // fs.Close();
                        }               
                    user.Profile = filePath; // שמירת הנתיב ב-DTO

                    }

                //add user 
                UserDto newUser =_userService.AddItem(user);

                    // save
                  //  transaction.Commit();
                    /*
                     * 
                     * 
                     * SendEmail(you are user...) pwd
                     * 
                     */
                    
                    // return Login(....)
                    return Ok($"user {newUser.UserName} registered seccessfully");
                }
                catch (Exception ex)
                {
                    //cancel transaction in case of errors
                   // transaction.Rollback();
                   //delete img
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            //}
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
                else //neither alphabet nor digit, meening a special char
                    foundChar = true;
            }
            return foundChar && foundLow && foundUp && foundNum;
        }

        // PUT api/<UserController>/5
        [Authorize]
        [HttpPut("{id}")]
        public UserDto Put(int id, [FromBody] UserDto updateUser)
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // בדוק אם ה-ID מהטוקן תואם ל-ID של המשתמש
            if (userIdFromToken != id.ToString())
            {
                throw new Exception("you cannot update user who is not you  yourself"); 
            }

            // אם יש תמונה חדשה, שמור אותה
            if (updateUser.File != null)
            {
                var filePath = Path.Combine(_directory, updateUser.File.FileName);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    updateUser.File.CopyTo(fs);
                }
               updateUser.Profile = filePath; // עדכון ה-DTO עם הנתיב
            }
            return _userService.Update(id, updateUser);
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            //Console.WriteLine("no deleting");
            _userService.Delete(id);
        }

        [HttpGet("profile-image/{id}")]
        public IActionResult GetProfileImage(int id)
        {
            UserDto user = _userService.Get(id);
            if (user != null && !string.IsNullOrEmpty(user.Profile))
            {
                if (System.IO.File.Exists(user.Profile))
                {
                    var fileBytes = System.IO.File.ReadAllBytes(user.Profile);
                    return File(fileBytes, "image/jpeg");
                }
                return NotFound("Profile image not found");
            }
            return NotFound("User or image not found");
        }

        //[HttpGet("profile-image/{id}")]
        //public IActionResult GetProfileImage(int id)
        //{
        //    UserDto user = _userService.Get(id);
        //    if (user != null && !string.IsNullOrEmpty(user.Profile))
        //    {
        //        return File(user.Image, "image/jpeg"); 
        //    }
        //    return NotFound();
        //}

    }
}
