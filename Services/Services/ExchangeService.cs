using AutoMapper;
using Repositories.Entity;
using Repositories.Interfaces;
using Services.Dtos;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using StatusExchange = Services.Dtos.StatusExchange;

namespace Services.Services
{
    public class ExchangeService : IExchangeExtensionService
    {
        private readonly IExchangeExtensionRepository _repository;
        private readonly IRepository<User> _userRepo;
        private readonly ITalentUserExtensionRepository _talentUserRepo;
        private readonly IMapper _mapper;

        public ExchangeService(IExchangeExtensionRepository repository, 
                                IRepository<User> userRep,  
                                    ITalentUserExtensionRepository talentUserRepo,
                                IMapper mapper)
        {
            this._repository = repository;
            this._userRepo = userRep;
            this._talentUserRepo = talentUserRepo;
            this._mapper = mapper;
        }

        public ExchangeDto AddItem(ExchangeDto item)
        {
            return _mapper.Map<ExchangeDto>(_repository.AddItem(_mapper.Map<Exchange>(item)));
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public ExchangeDto Get(int id)
        {
            return _mapper.Map<ExchangeDto>(_repository.Get(id));
        }
        public List<ExchangeDto> GetByUserId(int userId)
        {
            //SearchExhcahngesForUser(userId);
            return _mapper.Map<List<ExchangeDto>>(_repository.GetByUserId(userId));
        }

        public List<ExchangeDto> GetAll()
        {
            return _mapper.Map<List<ExchangeDto>>(_repository.GetAll());
        }

        public ExchangeDto Update(int id, ExchangeDto item)
        {
            return _mapper.Map<ExchangeDto>(_repository.Update(id, _mapper.Map<Exchange>(item)));
        }

        public void SearchExhcahngesForUser(int userId)
        {
            var exchanges = _repository.GetByUserId(userId);
            
            var user = _userRepo.Get(userId);
            
            var userTalents = _talentUserRepo.GetTalentsByUserId(userId);

            var offeredTalents = userTalents.Where(t => t.IsOffered).ToList();
            var requestedTalents = userTalents.Where(t => !t.IsOffered).ToList();

            var newExchanges = new List<Exchange>();

            foreach (var offered in offeredTalents)
            {
                var potentialPartners = _talentUserRepo.GetAll()
                    .Where(t => t.TalentId == offered.TalentId && !t.IsOffered && t.UserId != userId)
                    .ToList();

                foreach (var partner in potentialPartners)
                {
                    var matchingTalent = _talentUserRepo.GetAll()
                        .FirstOrDefault(t => t.UserId == userId && t.TalentId == partner.TalentId && t.IsOffered);

                    if (matchingTalent != null)
                    {
                        bool exists = exchanges.Any(e =>
                            (e.User1Id == userId && e.User2Id == partner.UserId) ||
                            (e.User1Id == partner.UserId && e.User2Id == userId));

                        if (!exists)
                        {
                            newExchanges.Add(new Exchange
                            {
                                User1Id = userId,
                                User2Id = partner.UserId,
                                Talent1Offered = offered.TalentId,
                                Talent2Offered = partner.TalentId,
                                Status = (Repositories.Entity.StatusExchange?)StatusExchange.NEW,
                                DateCreated = DateTime.Now
                            });
                        }
                    }
                }
                foreach (var ex in newExchanges)
                {
                    _repository.AddItem(ex);
                }
            }


        }

        public void UpdateUserExchanges(int userId, List<int> removedTalentIds, List<int> addedTalentIds)
        {
            // שליפת כל העסקאות של המשתמש
            var userExchanges = _repository.GetByUserId(userId);

            // איתור עסקאות חדשות או עסקאות שממתינות לתגובה שכוללות כישרונות שהוסרו
            var exchangesToDelete = userExchanges
                .Where(e => e.Status.Equals(StatusExchange.NEW) || e.Status.Equals(StatusExchange.WAITING)) // מסננים רק עסקאות חדשות או ממתינות
                .Where(e => removedTalentIds.Contains(e.Talent1Offered) || removedTalentIds.Contains(e.Talent2Offered)) // בודקים אם הכישרון בעסקה נמחק
                .ToList();

            // מחיקת העסקאות שמצאנו
            foreach (var exchange in exchangesToDelete)
            {
                _repository.Delete(exchange.Id);
            }

            // אם נוספו כישרונות חדשים, ניצור עסקאות חדשות בהתאם
            if (addedTalentIds.Any())
            {
                SearchExhcahngesForUser(userId);
            }
        }






    }
}

