using AutoMapper;
using Repositories.Entity;
using Repositories.Interfaces;
using Services.Dtos;
using Services.Interfaces;

namespace Services.Services
{
    public class TalentUserService : IService<TalentUserDto>
    {
        private readonly IRepository<TalentUser> _repository;
        private readonly IMapper _mapper;

        public TalentUserService(IRepository<TalentUser> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public TalentUserDto AddItem(TalentUserDto item)
        {
            var talentUser = _mapper.Map<TalentUser>(item);
            var addedTalentUser = _repository.AddItem(talentUser);
            return _mapper.Map<TalentUserDto>(addedTalentUser);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public TalentUserDto Get(int id)
        {
            var talentUser = _repository.Get(id);
            return _mapper.Map<TalentUserDto>(talentUser);
        }

        public List<TalentUserDto> GetAll()
        {
            var talentUsers = _repository.GetAll();
            return _mapper.Map<List<TalentUserDto>>(talentUsers);
        }

        public TalentUserDto Update(int id, TalentUserDto entity)
        {
            var talentUser = _mapper.Map<TalentUser>(entity);
            var updatedTalentUser = _repository.Update(id, talentUser);
            return _mapper.Map<TalentUserDto>(updatedTalentUser);
        }
    }
}