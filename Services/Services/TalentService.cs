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
    public class TalentService : IService<TalentDto>
    {
        private readonly IRepository<Talent> _repository;
        private readonly IMapper _mapper;

        public TalentService(IRepository<Talent> repository, IMapper mapper)
        {
            this._repository = repository;
            this._mapper = mapper;
        }

        public TalentDto AddItem(TalentDto item)
        {
            return _mapper.Map<TalentDto>(_repository.AddItem(_mapper.Map<Talent>(item)));
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public TalentDto Get(int id)
        {
            return _mapper.Map<TalentDto>(_repository.Get(id));
        }

        public List<TalentDto> GetAll()
        {
            return _mapper.Map<List<TalentDto>>(_repository.GetAll());
        }

        public TalentDto Update(int id, TalentDto item)
        {
            return _mapper.Map<TalentDto>(_repository.Update(id, _mapper.Map<Talent>(item)));
        }
    }
}
