using Repositories.Entity;
using Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IService<T>
    {
        List<T> GetAll();
        T Get(int id);
        T AddItem(T item);
        T Update(int id, T entity);
        void Delete(int id);


    }
    public interface ITalentExtensionService : IService<TalentDto>
    {
        List<TalentDto> GetByParentCategory(int parentCategoryId);
    }

    public interface ITalentUserExtensionService : IService<TalentUserDto>
    {
        List<TalentUserDto> AddTalentsForUser(List<TalentUserDto> talents);
        void DeleteTalentForUser(List<TalentUserDto> talents);
        List<TalentUserDto> GetTalentsByUserId(int userId);
        public void Delete(int userId, int talentId);

    }

    public interface IExchangeExtensionService : IService<ExchangeDto>
    {
        List<ExchangeDto> GetByUserId(int userId);
    }
}
