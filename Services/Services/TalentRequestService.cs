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
    public class TalentRequestService : Interfaces.IService<TalentRequestDto>
    {
        private readonly Repositories.Interfaces.IRepository<TalentRequest> _repository;
        private readonly IMapper _mapper;

        public TalentRequestService(Repositories.Interfaces.IRepository<TalentRequest> repository, IMapper mapper)
        {
            this._repository = repository;
            this._mapper = mapper;
        }

        public TalentRequestDto AddItem(TalentRequestDto item)
        {
            return _mapper.Map<TalentRequestDto>(_repository.AddItem(_mapper.Map<TalentRequest>(item)));
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
    }
}
