using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mock;
using Services.Dtos;
using Services.Interfaces;
using Services.Services;
using System.Data;
using WebAPI.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TalentController : ControllerBase, ITalentControllerService
    {
        private readonly IService<TalentDto> _talentService;
        private readonly IService<TalentRequestDto> _talentRequestService;
        //private readonly GiveAndGetDB _giveAndGetDB;
        

        public TalentController(
            IService<TalentDto> talentService, 
            IService<TalentRequestDto> talentRequestService)
        {
            _talentService = talentService;
            _talentRequestService = talentRequestService;
        }



        // GET: api/<TalentController>
        [HttpGet]
        public List<TalentDto> Get()
        {
            return _talentService.GetAll();
        }

        // GET api/<TalentController>/5
        [HttpGet("{id}")]
        public TalentDto Get(int id)
        {
            return _talentService.Get(id);
        }

        // POST api/<TalentController>
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult Post([FromForm] int id)
        {
            //token
            //using (var transaction = _giveAndGetDB.Database.BeginTransaction())
                try
                {
                    TalentRequestDto req = _talentRequestService.Get(id);

                    if (req == null)
                    {
                        return NotFound($"Request with ID {id} not found.");
                    }

                    // create new talent
                    TalentDto newTalent = new TalentDto(req.TalentName, req.ParentCategory);
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
                }
                catch (Exception ex)
                {
                    //cancle transaction in case of errors
                    //transaction.Rollback();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
        }



        // PUT api/<TalentController>/5
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public TalentDto Put(int id, [FromBody] TalentDto update)
        {
            //token
            return _talentService.Update(id, update);
        }

        // DELETE api/<TalentController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                _talentService.Delete(id);
                return NoContent(); //return no messege, we secceded
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // when talent is not defined
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}"); // any othor errors
            }
        }
    }
}

        
