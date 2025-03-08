using AutoMapper;
using Repositories.Entity;
using Repositories.Interfaces;
using Services.Dtos;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class UserService : IService<UserDto>
    {
        private readonly IRepository<User> _repository;
        private readonly IMapper _mapper;
        public UserService(IRepository<User> repository, IMapper mapper)
        {
            this._repository = repository;
            this._mapper = mapper;
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
    }
}
