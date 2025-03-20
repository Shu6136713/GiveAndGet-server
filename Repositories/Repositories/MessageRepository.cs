using Repositories.Entity;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories.Repositories
{
    public class MessageRepository : IMessageExtensionRepository
    {
        private readonly IContext context;

        public MessageRepository(IContext context)
        {
            this.context = context;
        }

        public Message AddItem(Message item)
        {
            context.Messages.Add(item);
            context.Save();
            return Get(item.Id);  // מחזיר את ההודעה אחרי שמירה
        }

        public void Delete(int id)
        {
            context.Messages.Remove(Get(id));  // מוחק הודעה לפי ID
            context.Save();
        }

        public Message Get(int id)
        {
            return context.Messages.FirstOrDefault(m => m.Id == id);  // מחזיר הודעה בודדת לפי ID
        }

        // לא משתמשים בו בצ'אט - לא נוגע
        public List<Message> GetAll()
        {
            throw new NotImplementedException();
        }

        // ✅ שליפה של כל ההודעות לפי ExchangeId - צ'אט של עסקה
        public List<Message> GetByExchangeId(int exchangeId)
        {
            return context.Messages
                .Where(m => m.ExchangeId == exchangeId)
                .OrderBy(m => m.Time)
                .ToList();
        }

        public Message Update(int id, Message entity)
        {
            throw new NotImplementedException();  // אין צורך בצ'אט - לרוב לא עורכים הודעות
        }
    }
}
