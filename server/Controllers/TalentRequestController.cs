using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Dtos;
using System.Collections.Generic;
using Mock;
using WebAPI.Interfaces;
using Services.Services;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TalentRequestController : ControllerBase
    {
        private readonly IService<TalentRequestDto> _talentRequestService;
        private readonly IService<TalentDto> _talentService;
        //private readonly GiveAndGetDB _giveAndGetDB;
        //private readonly ITalentControllerService _talentController;

        public TalentRequestController(IService<TalentRequestDto> talentRequestService,
             IService<TalentDto> talentService
            )
        {
            _talentRequestService = talentRequestService;
            _talentService = talentService;
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

        // POST api/<TalentRequestController>
        [Authorize(Roles = "user")]
        [HttpPost]
        public IActionResult Post([FromForm] TalentRequestDto req)
        {
            try
            {
                TalentRequestDto newReq = _talentRequestService.AddItem(req);
                /*
                 * 
                 * sent email to manager
                 * 
                 * 
                 */
                return Ok(newReq);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
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

                /*
                 * 
                 * SendEmail(req.UserEmail, "Talent Request Completed", $"Your requested talent '{req.TalentName}' has been added successfully!");
                 * 
                 * 
                 */

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
                return StatusCode(500, "An error occurred while processing the talent request.");
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
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
