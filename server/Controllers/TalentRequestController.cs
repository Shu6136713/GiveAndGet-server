using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Dtos;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TalentRequestController : ControllerBase
    {
        private readonly IService<TalentRequestDto> _talentRequestService;

        public TalentRequestController(IService<TalentRequestDto> talentRequestService)
        {
            _talentRequestService = talentRequestService;
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
        [HttpPost]
        public IActionResult Post([FromBody] TalentRequestDto req)
        {
            try
            {
                TalentRequestDto newRequest = _talentRequestService.AddItem(req);
                return Ok(newRequest);
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
            try
            {
                TalentRequestDto updated = _talentRequestService.Update(id, toUpdate);

                if (updated == null)
                {
                    return NotFound($"TalentRequest with ID {id} not found.");
                }

                return Ok($"Talent request {id} processed successfully.");
            }
            catch (Exception ex)
            {
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