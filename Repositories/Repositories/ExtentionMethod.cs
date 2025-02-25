using Microsoft.Extensions.DependencyInjection;
using Repositories.Entity;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Connection = Repositories.Entity.Connection;

namespace Repositories.Repositories
{
    public static class ExtentionMethod
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {


            services.AddScoped<IRepository<Comment>, CommentRepository>();
           // services.AddScoped<IRepository<Connection>, ConnectionRepository>();
            services.AddScoped<IRepository<Exchange>, ExchangeRepository>();
            services.AddScoped<IRepository<Message>, MessageRepository>();
            services.AddScoped<IRepository<Talent>, TalentRepository>();
            services.AddScoped<IRepository<TalentRequest>, TalentRequestRepository>();
            services.AddScoped<IRepository<User>, UserRepository>();

            return services;
        }
    }
}
