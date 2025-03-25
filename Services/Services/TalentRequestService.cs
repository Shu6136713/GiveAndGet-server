using AutoMapper;
using Repositories.Entity;
using Repositories.Interfaces;
using Services.Dtos;
using Services.Interfaces;
using System.Collections.Generic;

namespace Services.Services
{
    public class TalentRequestService : IService<TalentRequestDto>
    {
        private readonly IRepository<TalentRequest> _repository;
        private readonly IMapper _mapper;

        public TalentRequestService(IRepository<TalentRequest> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public TalentRequestDto AddItem(TalentRequestDto item)
        {
            var entity = _mapper.Map<TalentRequest>(item);
            var addedEntity = _repository.AddItem(entity);
            return _mapper.Map<TalentRequestDto>(addedEntity);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public TalentRequestDto Get(int id)
        {
            var entity = _repository.Get(id);
            return _mapper.Map<TalentRequestDto>(entity);
        }

        public List<TalentRequestDto> GetAll()
        {
            var entities = _repository.GetAll();
            return _mapper.Map<List<TalentRequestDto>>(entities);
        }

        public TalentRequestDto Update(int id, TalentRequestDto item)
        {
            var entity = _mapper.Map<TalentRequest>(item);
            var updatedEntity = _repository.Update(id, entity);
            return _mapper.Map<TalentRequestDto>(updatedEntity);
        }
    }
}
