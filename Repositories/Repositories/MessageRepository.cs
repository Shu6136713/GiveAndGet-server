using Repositories.Entity;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class MessageRepository : IRepository<Message>
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
            return Get(item.Id);
        }

        public void Delete(int id)
        {
            context.Messages.Remove(Get(id));
            context.Save();
        }

        public Message Get(int id)
        {
            return context.Messages.FirstOrDefault(m => m.Id == id);
        }

        public List<Message> GetAll()
        {
            throw new NotImplementedException();
        }

        public List<Message> GetByUserIds(int id1,int id2)
        {
            return context.Messages.
                Where(m => (m.FromId == id1 && m.ToId == id2) || (m.FromId == id2 && m.ToId == id1))
                .OrderBy(t=>t.Time)
                .ToList();
        }

        public Message Update(int id, Message entity)
        {
            throw new NotImplementedException();
        }
    }
}
