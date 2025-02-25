using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services.Dtos;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IService<UserDto> _userService;
        private readonly IConfiguration config;
        public LoginController(IService<UserDto> userService, IConfiguration config)
        {
            _userService = userService;
            this.config = config;
        }

        // GET: api/<LoginController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<LoginController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<LoginController>
        [HttpPost]
        public IActionResult Post([FromQuery] string userName, [FromQuery] string pwd)
        {
            UserDto user = Verify(userName, pwd);
            if (user == null)
                return BadRequest("user not found");
            string token = GenerateToken(user);
            return Ok(token);
        }
        private string GenerateToken(UserDto user)
        {
            //the code to encode in a bytes array
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            //encoding
            var carditional = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.IsAdmin==true ? "admin" : "user") // הוספת תביעה לתפקיד

            };
            var token = new JwtSecurityToken(
                config["Jwt:Issuer"], config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: carditional
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserDto Verify(string name, string pwd)
        {
            return _userService.GetAll().FirstOrDefault(u =>u.UserName == name && BCrypt.Net.BCrypt.Verify(pwd, u.HashPwd));
        }
        // PUT api/<LoginController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LoginController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
