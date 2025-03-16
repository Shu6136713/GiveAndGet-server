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
using StatusExchangeRep = Repositories.Entity.StatusExchange;


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

        public void SearchExchangesForUser(int userId)
        {
            var exchanges = _repository.GetByUserId(userId);
            var user = _userRepo.Get(userId);
            var userTalents = _talentUserRepo.GetTalentsByUserId(userId);

            var offeredTalents = userTalents.Where(t => t.IsOffered).ToList();
            var requestedTalents = userTalents.Where(t => !t.IsOffered).ToList();

            var allTalentUsers = _talentUserRepo.GetAll().ToList();
            List<Exchange> newExchanges = new List<Exchange>();

            ProcessTalentMatches(userId, user,userTalents,allTalentUsers,exchanges,newExchanges);
            //ProcessTalentMatches(userId, user, offeredTalents, allTalentUsers, exchanges, newExchanges, isOffered: true);
            //ProcessTalentMatches(userId, user, requestedTalents, allTalentUsers, exchanges, newExchanges, isOffered: false);

            foreach (var exchange in newExchanges)
            {
                _repository.AddItem(exchange);
            }
        }

        /// <summary>
        /// מחפש שותפים מתאימים לעסקאות תוך התחשבות בגילאים ומגדר.
        /// </summary>
        private void ProcessTalentMatches(int userId, User user, List<TalentUser> userTalents,
                                  List<TalentUser> allTalentUsers, List<Exchange> existingExchanges,
                                  List<Exchange> newExchanges)//, bool isOffered)
        {
            foreach (var talent in userTalents)
            {
                // חיפוש משתמשים שמבקשים את מה שאני מציע ולהפך
                var potentialPartners = allTalentUsers
                    .Where(t => t.TalentId == talent.TalentId && t.IsOffered != talent.IsOffered && t.UserId != userId)
                    .ToList();

                foreach (var partner in potentialPartners)
                {
                    var partnerUser = _userRepo.Get(partner.UserId);

                    // מניעת עסקה בין גברים לנשים
                    if (user.Gender != partnerUser.Gender) continue;

                    // בדיקה שהגילאים תואמים
                    if (!IsAgeMatch(user.Age, partnerUser.Age)) continue;

                    // ***תיקון כאן: עכשיו מחפשים התאמה הפוכה ולא זהה!***
                    var otherTalent = allTalentUsers.FirstOrDefault(t =>/**//**/
                        t.UserId==partner.UserId && 
                        (
                            userTalents.FirstOrDefault(ut=> ut.TalentId == t.TalentId && ut.IsOffered != talent.IsOffered) != null
                        )
                        && t.IsOffered != talent.IsOffered); // 🔄 הפוך ממה שמחפשים בצד השני
                    
                    if (otherTalent != null)
                    {
                        bool exists = existingExchanges.Any(e =>
                            (e.User1Id == userId && e.User2Id == partner.UserId) ||
                            (e.User1Id == partner.UserId && e.User2Id == userId));

                        if (!exists)
                        {
                            newExchanges.Add(new Exchange
                            {
                                User1Id = userId,
                                User2Id = partner.UserId,/***********/
                                Talent1Offered = talent.IsOffered ? talent.TalentId : otherTalent.TalentId,
                                Talent2Offered = talent.IsOffered ? otherTalent.TalentId : talent.TalentId,
                                Status = StatusExchangeRep.NEW,
                                DateCreated = DateTime.Now
                            });
                        }
                    }
                }
            }
        }


        /// <summary>
        /// קובע אם שני גילאים יכולים להיות בהתאמה עסקית.
        /// </summary>
        private bool IsAgeMatch(int age1, int age2)
        {
            int minAge = Math.Min(age1, age2);
            int maxAge = Math.Max(age1, age2);

            if (maxAge <= 5) return minAge >= 3; // פעוטות יכולים עם פעוטות  
            if (maxAge <= 10) return minAge >= 5;
            if (maxAge <= 15) return minAge >= 10;
            if (maxAge <= 20) return minAge >= 15;
            if (maxAge <= 30) return minAge >= 18;
            if (maxAge <= 50) return minAge >= 25;
            return minAge >= 40; // מבוגרים יכולים עם בני גילם  
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
            
            SearchExchangesForUser(userId);
            
        }






    }
}

