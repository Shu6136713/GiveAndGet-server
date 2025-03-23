using BCrypt.Net;
using Repositories.Entity;
using Repositories.Interfaces;
using System.Linq;

namespace Repositories.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        private readonly IContext _context;

        public LoginRepository(IContext context)
        {
            _context = context;
        }

        public User VerifyUser(string userName, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.HashPwd))
            {
                return user;
            }
            return null;
        }
    }
}