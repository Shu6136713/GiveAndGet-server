using Services.Dtos;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class ChatService
    {
        private readonly IMessageExtensionService _messageService;

        public ChatService(IMessageExtensionService messageService)
        {
            _messageService = messageService;
        }

        public async Task SaveMessageAsync(int exchangeId, int fromUserId, string text)
        {
            var msgDto = new MessageDto
            {
                ExchangeId = exchangeId,
                FromId = fromUserId,
                Text = text,
                Time = DateTime.Now
            };
            await _messageService.AddItemAsync(msgDto);
        }

        public async Task<List<MessageDto>> GetChatHistoryAsync(int exchangeId)
        {
            return await _messageService.GetByExchangeIdAsync(exchangeId);
        }
    }


}
