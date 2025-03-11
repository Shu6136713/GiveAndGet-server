using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Dtos;
using System.Collections.Generic;
using Services.Services;
using Microsoft.AspNetCore.Authorization;
using Repositories.Entity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TalentRequestController : ControllerBase
    {
        private readonly IService<TalentRequestDto> _talentRequestService;
        private readonly IService<TalentDto> _talentService;
        private readonly EmailService _emailService;
        private readonly IService<UserDto> _userService;
        public TalentRequestController(IService<TalentRequestDto> talentRequestService,
                                       IService<TalentDto> talentService,
                                       IService<UserDto> userService,  
                                       EmailService emailService)
        {
            _talentRequestService = talentRequestService;
            _talentService = talentService;
            _userService = userService;
            _emailService = emailService;
        }


        // GET: api/<TalentRequestController>
        [Authorize(Roles = "admin")]
        [HttpGet]
        public List<TalentRequestDto> Get()
        {
            return _talentRequestService.GetAll();
        }

        // GET api/<TalentRequestController>/5
        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public TalentRequestDto Get(int id)
        {
            return _talentRequestService.Get(id);
        }


        // POST api/<TalentRequestController>/5
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
                    // הודעת שגיאה במקרה של כשלון בשליחת המייל
                    Console.WriteLine($"Error sending email to {admin.Email}: {ex.Message}");
                }
            }
        }





        // PUT api/<TalentRequestController>/5
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] TalentRequestDto toUpdate)
        {
            //using (var transaction = _giveAndGetDB.Database.BeginTransaction())
            //{
            try
            {
                // עדכון הבקשה ב-TalentRequestService
                TalentRequestDto updated = _talentRequestService.Update(id, toUpdate);

                if (updated == null)
                {
                    return NotFound($"TalentRequest with ID {id} not found.");
                }

                //move from talent request to talent

                // create new talent
                TalentDto newTalent = new TalentDto(updated.TalentName, updated.ParentCategory);
                _talentService.AddItem(newTalent);

                UserDto user = _userService.Get(toUpdate.UserId);
                if (user != null)
                {
                    _emailService.SendEmail("משתמש יקר",
                                            "בקשתך להוספת כשרון " + updated.TalentName + "אושרה\nיום נעים",
                                            user.Email);
                }

                // delete request
                _talentRequestService.Delete(id);

                // save
                //transaction.Commit();

                return Ok($"Talent request {id} processed successfully.");

                // קריאה ל-Post בקונטרולר של Talent
                //IActionResult result = PostTalent(updated.Id);

                // אם קריאת ה-Post הצליחה, מחויב את הטרנזקציה
                //if (result is OkObjectResult okResult)
                //{
                //    //transaction.Commit(); // מחויב את השינויים
                //    return Ok(result + " updated");
                //}

                // במקרה של שגיאה בקריאת ה-Post, מחזירים את השגיאה
                //transaction.Rollback(); // מבטל את כל השינויים אם קריאת ה-Post נכשלת
                //return StatusCode(500, "An error occurred while processing the talent request.");
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                UserDto user = _userService.Get(toUpdate.UserId);
                if (user != null)
                {
                    _emailService.SendEmail("משתמש יקר",
                                            "לצערנו בקשתך להוספת כשרון " + toUpdate.TalentName + "סורבה\nיום נעים",
                                            user.Email);
                }
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE api/<TalentRequestController>/5
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _talentRequestService.Delete(id);
        }
    }
}
