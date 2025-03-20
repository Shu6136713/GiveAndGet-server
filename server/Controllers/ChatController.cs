using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("history/{exchangeId}")]
        public async Task<IActionResult> GetChatHistory(int exchangeId)
        {
            var history = await _chatService.GetChatHistoryAsync(exchangeId);
            return Ok(history);
        }
    }

}
