using Microsoft.EntityFrameworkCore;
using Repositories.Entity;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories.Repositories
{
    public class TalentUserRepository : IRepository<TalentUser>, ITalentUserRepository
    {
        private readonly IContext _context;

        public TalentUserRepository(IContext context)
        {
            _context = context;
        }

        public void AddTalentsForUser(int userId, List<TalentUser> talents)
        {
            var existingTalents = _context.TalentUser.Where(t => t.UserId == userId).ToList();
            _context.TalentUser.RemoveRange(existingTalents);

            foreach (var talent in talents)
            {
                _context.TalentUser.Add(new TalentUser
                {
                    UserId = userId,
                    TalentId = talent.TalentId,
                    IsOffered = talent.IsOffered
                });
            }
            _context.Save();
        }

        public TalentUser AddItem(TalentUser item)
        {
            _context.TalentUser.Add(item);
            _context.Save();
            return item;
        }

        public void Delete(int id)
        {
            var talentUser = _context.TalentUser.Find(id);
            if (talentUser != null)
            {
                _context.TalentUser.Remove(talentUser);
                _context.Save();
            }
        }

        public TalentUser Get(int id)
        {
            return _context.TalentUser.Find(id);
        }

        public List<TalentUser> GetAll()
        {
            return _context.TalentUser.ToList();
        }

        public TalentUser Update(int id, TalentUser entity)
        {
            var talentUser = _context.TalentUser.Find(id);
            if (talentUser != null)
            {
                talentUser.TalentId = entity.TalentId;
                talentUser.IsOffered = entity.IsOffered;
                _context.Save();
            }
            return talentUser;
        }

        public List<TalentUser> GetTalentsByUserId(int userId)
        {
            return _context.TalentUser.Where(t => t.UserId == userId).ToList();
        }
    }
}