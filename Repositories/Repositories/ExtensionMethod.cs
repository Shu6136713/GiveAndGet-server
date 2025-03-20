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
    public static class ExtensionMethod
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IMessageExtensionRepository<Comment>, CommentRepository>();
            services.AddScoped<IMessageExtensionRepository<Exchange>, ExchangeRepository>();
            services.AddScoped<IMessageExtensionRepository<Message>, MessageRepository>();
            services.AddScoped<IMessageExtensionRepository<Talent>, TalentRepository>();
            services.AddScoped<IMessageExtensionRepository<TalentRequest>, TalentRequestRepository>();
            services.AddScoped<IMessageExtensionRepository<User>, UserRepository>();
            services.AddScoped<IMessageExtensionRepository<TalentUser>, TalentUserRepository>();
            services.AddScoped<ITalentExtensionRepository, TalentRepository>();
            services.AddScoped<ITalentUserExtensionRepository, TalentUserRepository>();
            services.AddScoped<IExchangeExtensionRepository, ExchangeRepository>();
            services.AddScoped<IMessageExtensionRepository, MessageRepository>();
            return services;
        }
    }
}
