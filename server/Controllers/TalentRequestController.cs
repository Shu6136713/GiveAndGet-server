using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Dtos;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Repositories.Entity;
using System.Linq;
using System;
using Services.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TalentRequestController : ControllerBase
    {
        private readonly IService<TalentRequestDto> _talentRequestService;
        private readonly IService<TalentDto> _talentService;
        private readonly IEmailService _emailService;
        private readonly IService<UserDto> _userService;

        public TalentRequestController(
            IService<TalentRequestDto> talentRequestService,
            IService<TalentDto> talentService,
            IService<UserDto> userService,
            IEmailService emailService)
        {
            _talentRequestService = talentRequestService;
            _talentService = talentService;
            _userService = userService;
            _emailService = emailService;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public List<TalentRequestDto> Get()
        {
            return _talentRequestService.GetAll();
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public TalentRequestDto Get(int id)
        {
            return _talentRequestService.Get(id);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TalentRequestDto req)
        {
            try
            {
                TalentRequestDto newRequest = _talentRequestService.AddItem(req);
                UserDto requestingUser = _userService.Get(newRequest.UserId);
                string subject = $"משתמש {requestingUser.UserName} ביקש להוסיף כשרון למערכת.";
                string body = "אנא הכנס בהקדם לדף ניהול בקשות ההוספה ואשר / דחה את הבקשה.\nיום נעים!";
                NotifyAdmins(subject, body);
                return Ok(newRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        private void NotifyAdmins(string subject, string body)
        {
            List<UserDto> adminUsers = _userService.GetAll().Where(user => (bool)user.IsAdmin).ToList();
            foreach (UserDto admin in adminUsers)
            {
                try
                {
                    _emailService.SendEmail(subject, body, admin.Email);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email to {admin.Email}: {ex.Message}");
                }
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] TalentRequestDto toUpdate)
        {
            try
            {
                TalentRequestDto updated = _talentRequestService.Update(id, toUpdate);
                if (updated == null)
                {
                    return NotFound($"TalentRequest with ID {id} not found.");
                }
                TalentDto newTalent = new TalentDto(updated.TalentName, updated.ParentCategory);
                _talentService.AddItem(newTalent);
                UserDto user = _userService.Get(toUpdate.UserId);
                if (user != null)
                {
                    _emailService.SendEmail("משתמש יקר", $"בקשתך להוספת כשרון {updated.TalentName} אושרה\nיום נעים", user.Email);
                }
                _talentRequestService.Delete(id);
                return Ok($"Talent request {id} processed successfully.");
            }
            catch (Exception ex)
            {
                UserDto user = _userService.Get(toUpdate.UserId);
                if (user != null)
                {
                    _emailService.SendEmail("משתמש יקר", $"לצערנו בקשתך להוספת כשרון {toUpdate.TalentName} סורבה\nיום נעים", user.Email);
                }
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            TalentRequestDto toDelete= _talentRequestService.Get(id);
            _talentRequestService.Delete(id);

            UserDto user = _userService.Get(toDelete.UserId);
            if (user != null)
            {
                _emailService.SendEmail("משתמש יקר", $"לצערנו בקשתך להוספת כשרון {toDelete.TalentName} סורבה\nיום נעים", user.Email);
            }
            return Ok($"Talent request {id} deleted successfully.");
        }
    }
}
