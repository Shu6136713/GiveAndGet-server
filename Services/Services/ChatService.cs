using Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class ChatService
    {
        private readonly MessageService _messageService;

        public ChatService(MessageService messageService)
        {
            _messageService = messageService;
        }

        public void SaveMessage(int exchangeId, int fromUserId, string text)
        {
            var msgDto = new MessageDto
            {
                ExchangeId = exchangeId,
                FromId = fromUserId,
                Text = text,
                Time = DateTime.Now
            };
            _messageService.AddItem(msgDto);
        }

        public List<MessageDto> GetChatHistory(int exchangeId)
        {
            return _messageService.GetByExchange(exchangeId);
        }
    }

}
