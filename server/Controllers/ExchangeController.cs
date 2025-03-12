using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Dtos;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeController : ControllerBase
    {
        private readonly IExchangeExtensionService _exchangeService;

        public ExchangeController(IExchangeExtensionService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        // GET: api/<ExchangeController>
        //[HttpGet]
        //public List<ExchangeDto> Get()
        //{
        //    return _exchangeService.GetAll();
        //}

        // GET api/<ExchangeController>/5
         [Authorize]
        [HttpGet("{id}")]
        public ExchangeDto Get(int id)
        { 
            return _exchangeService.Get(id);
        }

        //[Authorize]
        [HttpGet("searchByUser")]
        public List<ExchangeDto> SearchDealsByUser(/*[FromQuery] DealSearchDto searchDto*/ [FromQuery] int userId)
        {
            return _exchangeService.GetByUserId(userId);
            //return _exchangeService.GetAll().Where(e => e.User1Id == userId || e.User2Id == userId).ToList();
            //try
            //{
            //    // מקבל את פרטי החיפוש של המשתמש
            //    var results = _exchangeService.SearchExhcahngesForUser(searchDto, userId);

            //    // אם לא נמצאו תוצאות
            //    if (results == null || !results.Any())
            //    {
            //        return NotFound("No deals found matching your criteria.");
            //    }

            //    // מחזיר את התוצאות
            //    return Ok(results);
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, $"An error occurred: {ex.Message}");
            //}
        }


        // POST api/<ExchangeController>
        [Authorize]
        [HttpPost]
        public IActionResult Post([FromBody] ExchangeDto exchange)
        {
            try
            {
                var createdExchange = _exchangeService.AddItem(exchange);
                return Ok(createdExchange);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT api/<ExchangeController>/5
        [Authorize]
        [HttpPut("{id}")]
        public ExchangeDto Put(int id, [FromBody] ExchangeDto update)
        {
            return _exchangeService.Update(id, update);
        }

        // DELETE api/<ExchangeController>/5
        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _exchangeService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
