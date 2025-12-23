using AutoMapper;
using Newtonsoft.Json;
using Repositories.Entity;
using Repositories.Interfaces;
using Repositories.Repositories;
using Services.Dtos;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Gender = Services.Dtos.Gender;

namespace Services.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IMapper _mapper;
        private readonly ITalentUserExtensionService _talentUserService;
        private readonly IExchangeExtensionService _exchangeService;
        private readonly ITalentExtensionService _talentService;
        public static string _directory = Path.Combine(Environment.CurrentDirectory, "Images");
        private const int TOP_USERS_COUNT = 5;


        public UserService(IRepository<User> repository,
                           IMapper mapper,
                           ITalentUserExtensionService talentUserService,
                           IExchangeExtensionService exchangeService,
                           ITalentExtensionService talentExtensionService)
        {
            _repository = repository;
            _mapper = mapper;
            _talentUserService = talentUserService;
            _exchangeService = exchangeService;
            _talentService = talentExtensionService;
        }

        public UserDto AddItem(UserDto item)
        {
            return _mapper.Map<UserDto>(_repository.AddItem(_mapper.Map<User>(item)));
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public UserDto Get(int id)
        {
            return _mapper.Map<UserDto>(_repository.Get(id));
        }

        public List<UserDto> GetAll()
        {
            return _mapper.Map<List<UserDto>>(_repository.GetAll());
        }

        public UserDto Update(int id, UserDto item)
        {
            return _mapper.Map<UserDto>(_repository.Update(id, _mapper.Map<User>(item)));
        }

        public UserDto GetUserProfile(ClaimsPrincipal user, string host)
        {
            var userIdFromToken = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdFromToken))
            {
                throw new UnauthorizedAccessException("Token is missing or invalid");
            }

            var userDto = Get(int.Parse(userIdFromToken));
            if (userDto == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (!string.IsNullOrEmpty(userDto.Profile))
            {
                userDto.ProfileImageUrl = $"{host}/api/User/profile-image/{userDto.Id}";
            }

            return userDto;
        }

        public UserDto AddUser(UserDto user, string talents)
        {
            ValidateUser(user);

            string pwd = user.HashPwd;

            // check if the password is valid
            if (!CheckIfValidatePwd(pwd))
                throw new ArgumentException("Password must contain upper and lower case letters, numbers, and special characters.");

            // hashing pwd
            string hashPwd = PasswordManagerService.HashPassword(pwd);
            user.HashPwd = hashPwd;

            // manage profile img
            if (user.File != null)
            {
                if (!Directory.Exists(_directory))
                {
                    Directory.CreateDirectory(_directory);
                }
                var filePath = Path.Combine(_directory, user.File.FileName);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    user.File.CopyTo(fs);
                }
                user.Profile = user.File.FileName;
            }

            // add user
            UserDto newUser = AddItem(user);

            // add talents to the user if exits
            if (talents != "[]")
            {
                List<TalentUserDto> talentList = JsonConvert.DeserializeObject<List<TalentUserDto>>(talents);
                foreach (TalentUserDto t in talentList)
                {
                    t.UserId = newUser.Id;
                }
                _talentUserService.AddTalentsForUser(talentList);
                _exchangeService.SearchExchangesForUser(newUser.Id);
            }

            return newUser;
        }

        public UserDto UpdateUser(int id, UserDto updateUser, string talents)
        {
            ValidateUser(updateUser);


            // if password is updated, validate it
            if (!string.IsNullOrEmpty(updateUser.HashPwd))
            {
                if (!CheckIfValidatePwd(updateUser.HashPwd))
                    throw new ArgumentException("Password must contain upper and lower case letters, numbers, and special characters.");

                updateUser.HashPwd = PasswordManagerService.HashPassword(updateUser.HashPwd);
            }

            // if a profile img is updated- put the new in the server
            if (updateUser.File != null)
            {
                var filePath = Path.Combine(_directory, updateUser.File.FileName);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    updateUser.File.CopyTo(fs);
                }
                updateUser.Profile = updateUser.File.FileName;
            }

            //get the current talents list of the user
            var currentTalents = _talentUserService.GetTalentsByUserId(id)
                                  .Select(t => t.TalentId).ToList();

            // Convert the received JSON string to a list of user talents
            List<TalentUserDto> talentUserList_new = !string.IsNullOrEmpty(talents) ?
                JsonConvert.DeserializeObject<List<TalentUserDto>>(talents) : new List<TalentUserDto>();

            // The new TalentId list after filtering
            var newTalents = talentUserList_new.Select(t => t.TalentId).Distinct().ToList();

            // detect deleted talents (were exist, not exist in the new list)
            var removedTalentIds = currentTalents.Except(newTalents).ToList();

            // detect new talents (were not exist, exist in the new list)
            var addedTalentIds = newTalents.Except(currentTalents).ToList();

            // detect talents that existed but whose status (IsOffered) may have changed
            var existingTalents = currentTalents.Intersect(newTalents).ToList();
            foreach (var talentId in existingTalents)
            {
                var newTalent = talentUserList_new.FirstOrDefault(t => t.TalentId == talentId);
                if (newTalent != null)
                {
                    _talentUserService.UpdateIsOffered(id, talentId, newTalent.IsOffered);
                }
            }

            // delete the deleted talents
            foreach (var talentId in removedTalentIds)
            {
                _talentUserService.Delete(id, talentId);
            }

            // add the new if there are
            if (addedTalentIds.Any())
            {
                var addedTalents = talentUserList_new
                    .Where(t => addedTalentIds.Contains(t.TalentId)) // מסננים רק את הכישרונות החדשים
                    .Select(t => new TalentUserDto { UserId = id, TalentId = t.TalentId, IsOffered = t.IsOffered }) // שומרים את ה- IsOffered
                    .ToList();

                _talentUserService.AddTalentsForUser(addedTalents);
            }

            // update the user details
            UserDto updatedUser = Update(id, updateUser);

            // update exchanges according to the talents' changes
            _exchangeService.UpdateUserExchanges(id, removedTalentIds, addedTalentIds);

            return updatedUser;
        }

        public UserDto UpdateUserScore(int id, int action)
        {
            var user = _repository.Get(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            // points to increase or decrease
            int pointsToChange = 0;

            switch (action)
            {
                case 0: // decrease
                    pointsToChange = -5;
                    break;
                case 1: // increase
                    pointsToChange = 10;
                    break;
                case 2: // cancel decreasment
                    pointsToChange = 5;
                    break;
                case 3: // cancel increasment
                    pointsToChange = -10;
                    break;
                default:
                    throw new ArgumentException("Invalid action parameter.");
            }

            user.Score += pointsToChange;
            if (user.Score < 0) user.Score = 0; // ensure that score won't be less than 0
            
            _repository.Update(id, user);
            return _mapper.Map<UserDto>(user);
        }

        public byte[] GetProfileImage(int id)
        {
            UserDto user = Get(id);
            if (user != null && !string.IsNullOrEmpty(user.Profile))
            {
                var filePath = Path.Combine(_directory, user.Profile);
                if (System.IO.File.Exists(filePath))
                {
                    return System.IO.File.ReadAllBytes(filePath);
                }
            }
            throw new FileNotFoundException("User or image not found");
        }

        private bool CheckIfValidatePwd(string pwd)
        {
            if (pwd.Length < 8 || pwd.Length > 20)
                return false;

            bool foundUp = false, foundLow = false, foundChar = false, foundNum = false;
            for (int i = 0; i < pwd.Length && !(foundChar && foundLow && foundUp && foundNum); i++)
            {
                if (Char.IsUpper(pwd[i]))
                    foundUp = true;
                else if (Char.IsLower(pwd[i]))
                    foundLow = true;
                else if (Char.IsDigit(pwd[i]))
                    foundNum = true;
                else
                    foundChar = true;
            }
            return foundChar && foundLow && foundUp && foundNum;
        }

        private void ValidateUser(UserDto user)
        {
            ValidatePhoneNumber(user.PhoneNumber);
            ValidateEmail(user.Email);
            ValidateAge(user.Age);
            ValidateGender(user.Gender);
        }

        private void ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number is required.");
            }

            // Example validation for phone number (Israel phone number format)
            var phoneRegex = new Regex(@"^(\+972|0)([23489]|5[0248]|77)[1-9]\d{6}$");
            if (!phoneRegex.IsMatch(phoneNumber))
            {
                throw new ArgumentException("Invalid phone number.");
            }
        }

        private void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.");
            }

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
            {
                throw new ArgumentException("Invalid email address.");
            }
        }

        private void ValidateAge(int age)
        {
            if (age < 7 || age > 120)
            {
                throw new ArgumentException("Age must be between 7 and 120.");
            }
        }

        private void ValidateGender(Gender gender)
        {
            if (!Enum.IsDefined(typeof(Gender), gender))
            {
                throw new ArgumentException("Invalid gender.");
            }
        }

        public List<TopUserDto> GetTopUsers()
        {
            var users = _repository.GetAll();

            var topUsers = users
                .OrderByDescending(u => u.Score)
                .Take(TOP_USERS_COUNT)
                .ToList();

            return _mapper.Map<List<TopUserDto>>(topUsers);
        }

        public TopUserDto GetNotSecret(int id)
        {
            User u = _repository.Get(id);
            if (u == null)
            {
                throw new ArgumentException("user not found");
            }
            UserDto user = _mapper.Map<UserDto>(u);
            return new TopUserDto(user.UserName, user.Score, user.Desc, user.Profile);
        }
    }
}
