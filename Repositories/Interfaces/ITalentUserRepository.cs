using Repositories.Entity;

namespace Repositories.Interfaces
{
    public interface ITalentUserRepository : IRepository<TalentUser>
    {
        void AddTalentsForUser(int userId, List<TalentUser> talents);
        List<TalentUser> GetTalentsByUserId(int userId); // פונקציה להוספת כישרונות לפי מזהה משתמש
    }
}