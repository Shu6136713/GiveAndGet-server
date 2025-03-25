using AutoMapper;
using Repositories.Entity;
using Repositories.Interfaces;
using Services.Dtos;
using Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Services.Services
{
    public class TalentRequestService : IService<TalentRequestDto>
    {
        private readonly IRepository<TalentRequest> _repository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public TalentRequestService(IRepository<TalentRequest> repository, IMapper mapper, IUserService userService, IEmailService emailService)
        {
            this._repository = repository;
            this._mapper = mapper;
            this._userService = userService;
            this._emailService = emailService;
        }

        public TalentRequestDto AddItem(TalentRequestDto item)
        {
            TalentRequestDto newRequest = _mapper.Map<TalentRequestDto>(_repository.AddItem(_mapper.Map<TalentRequest>(item)));
            UserDto requestingUser = _userService.Get(newRequest.UserId);

            string subject = $"משתמש {requestingUser.UserName} ביקש להוסיף כשרון למערכת.";
            string body = "אנא הכנס בהקדם לדף ניהול בקשות ההוספה ואשר / דחה את הבקשה.\nיום נעים!";

            NotifyAdmins(subject, body);

            return newRequest;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public TalentRequestDto Get(int id)
        {
            return _mapper.Map<TalentRequestDto>(_repository.Get(id));
        }

        public List<TalentRequestDto> GetAll()
        {
            return _mapper.Map<List<TalentRequestDto>>(_repository.GetAll());
        }

        public TalentRequestDto Update(int id, TalentRequestDto item)
        {
            return _mapper.Map<TalentRequestDto>(_repository.Update(id, _mapper.Map<TalentRequest>(item)));
        }

        //private void NotifyAdmins(string subject, string body)
        //{
        //    List<UserDto> adminUsers = _userService.GetAll().Where(user => (bool)user.IsAdmin).ToList();
        //    foreach (UserDto admin in adminUsers)
        //    {
        //        try
        //        {
        //            _emailService.SendEmail(subject, body, admin.Email);
        //        }
        //        catch (Exception ex)
        //        {
        //            // הודעת שגיאה במקרה של כשלון בשליחת המייל
        //            Console.WriteLine($"Error sending email to {admin.Email}: {ex.Message}");
        //        }
        //    }
        //}
    }
}