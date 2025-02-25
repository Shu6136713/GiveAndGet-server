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

namespace Services.Services
{
    public class ExchangeService : IService<ExchangeDto>
    {
        private readonly IRepository<Exchange> _repository;
        private readonly IMapper _mapper;

        public ExchangeService(IRepository<Exchange> repository, IMapper mapper)
        {
            this._repository = repository;
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

        public List<ExchangeDto> GetAll()
        {
            return _mapper.Map<List<ExchangeDto>>(_repository.GetAll());
        }

        public ExchangeDto Update(int id, ExchangeDto item)
        {
            return _mapper.Map<ExchangeDto>(_repository.Update(id, _mapper.Map<Exchange>(item)));
        }

        public List<ExchangeDto> SearchExhcahngesForUser(/*SerchDto searchDto,*/ int userId)
        {
            return null;
        }
    }
}
