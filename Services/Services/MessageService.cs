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
    public class MessageService : IService<MessageDto>
    {
        private readonly IRepository<Message> _repository;
        private readonly IMapper _mapper;

        public MessageService(IRepository<Message> repository, IMapper mapper)
        {
            this._repository = repository;
            this._mapper = mapper;
        }

        public MessageDto AddItem(MessageDto item)
        {
            return _mapper.Map<MessageDto>(_repository.AddItem(_mapper.Map<Message>(item)));
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public MessageDto Get(int id)
        {
            return _mapper.Map<MessageDto>(_repository.Get(id));
        }

        public List<MessageDto> GetAll()
        {
            return _mapper.Map<List<MessageDto>>(_repository.GetAll());
        }

        public MessageDto Update(int id, MessageDto item)
        {
            return _mapper.Map<MessageDto>(_repository.Update(id, _mapper.Map<Message>(item)));
        }
    }
}
