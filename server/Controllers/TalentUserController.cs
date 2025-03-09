using Microsoft.AspNetCore.Mvc;
using Services.Dtos;
using Services.Interfaces;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TalentUserController : ControllerBase
    {
        private readonly ITalentUserExtensionService _talentUserService;

        public TalentUserController(ITalentUserExtensionService talentUserService)
        {
            _talentUserService = talentUserService;
        }

        
        [HttpPost("addTalents")]
        public ActionResult<List<TalentUserDto>> AddTalentsForUser([FromBody] List<TalentUserDto> talents)
        {
            if (talents == null || !talents.Any())
                return BadRequest("The talent list cannot be empty.");

            var updatedTalents = _talentUserService.AddTalentsForUser(talents);
            return Ok(updatedTalents);
        }

       
        [HttpGet("getTalents/{userId}")]
        public ActionResult<List<TalentUserDto>> GetTalentsByUserId(int userId)
        {
            var talents = _talentUserService.GetTalentsByUserId(userId);
            if (talents == null || !talents.Any())
                return NotFound("No talents found for the given user.");

            return Ok(talents);
        }

        [HttpPost("addTalent")]
        public ActionResult<TalentUserDto> AddTalent([FromBody] TalentUserDto talent)
        {
            if (talent == null)
                return BadRequest("Talent data is required.");

            var addedTalent = _talentUserService.AddItem(talent);
            return Ok(addedTalent);
        }

        [HttpDelete("delete/{id}")]
        public IActionResult DeleteTalent(int id)
        {
            _talentUserService.Delete(id);
            return NoContent();
        }

        [HttpPut("update/{id}")]
        public ActionResult<TalentUserDto> UpdateTalent(int id, [FromBody] TalentUserDto talent)
        {
            if (talent == null)
                return BadRequest("Talent data is required.");

            var updatedTalent = _talentUserService.Update(id, talent);
            return Ok(updatedTalent);
        }
    }
}
