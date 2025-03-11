using Repositories.Entity;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class ExchangeRepository : IExchangeExtensionRepository
    {
        private readonly IContext context;
        public ExchangeRepository(IContext context)
        {
            this.context = context;
        }

        public Exchange AddItem(Exchange item)
        {
            context.Exchanges.Add(item);
            context.Save();
            return Get(item.Id);
        }

        public void Delete(int id)
        {
            context.Exchanges.Remove(Get(id));
            context.Save();
        }

        public Exchange Get(int id)
        {
            return context.Exchanges.FirstOrDefault(e => e.Id == id);
        }
        public List<Exchange> GetByUserId(int userId)
        {
            return context.Exchanges.Where(e=> e.User1Id==userId || e.User2Id==userId).ToList();
        }

        public List<Exchange> GetAll()
        {
            return context.Exchanges.ToList();
        }

        public Exchange Update(int id, Exchange entity)
        {
            Exchange e= Get(id);
            e.User1Id = entity.User1Id;
            e.User2Id = entity.User2Id;
            e.Status= entity.Status;
            e.Talent1Offered= entity.Talent1Offered;
            e.Talent2Offered = entity.Talent2Offered;
            e.DateCompleted= entity.DateCompleted;
            context.Save();
            return Get(id);

        }
    }
}
