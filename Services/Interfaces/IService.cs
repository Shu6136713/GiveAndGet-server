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
}
