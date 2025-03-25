using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.Dtos;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILoginService _loginService;


        public UserController(IUserService userService, ILoginService loginService)
        {
            _userService = userService;
            _loginService = loginService;
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
            try
            {
                var userProfile = _userService.GetUserProfile(User, $"{Request.Scheme}://{Request.Host}");
                return Ok(userProfile);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST api/<UserController>
        [HttpPost]
        public IActionResult Post([FromForm] UserDto user, [FromForm] string talents)
        {
            try
            {
                Console.WriteLine($"User: {JsonConvert.SerializeObject(user)}, Talents: {talents}");
                var newUser = _userService.AddUser(user, talents);
                return Ok(newUser);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT api/<UserController>/5
        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] UserDto updateUser, [FromForm] string talents)
        {
            try
            {
                if (!_loginService.ValidateUserId(User, id))
                {
                    return Unauthorized("User ID does not match the token.");
                }
                Console.WriteLine(talents);
                var updatedUser = _userService.UpdateUser(id, updateUser, talents);
                return Ok(updatedUser);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE api/<UserController>/5
        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (!_loginService.ValidateUserId(User, id))
                {
                    return Unauthorized("User ID does not match the token.");
                }
                _userService.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("profile-image/{id}")]
        public IActionResult GetProfileImage(int id)
        {
            try
            {
                var fileBytes = _userService.GetProfileImage(id);
                return File(fileBytes, "image/jpeg");
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update-score/{id}")]
        public IActionResult UpdateScore(int id, [FromBody] int action)
        {
            try
            {
                var updatedUser = _userService.UpdateUserScore(id, action);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("top")]
        public List<TopUserDto> GetTopUsers()
        {
            return _userService.GetTopUsers();
        }
    }
}