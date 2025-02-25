using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Entity;
using Repositories.Repositories;
using Services.Dtos;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Services.Services
{
    public static class ExtentionService
    {
        public static IServiceCollection AddServiceExtension(this IServiceCollection services)
        {
            services.AddRepository();
            services.AddScoped<IService<CommentDto>, CommentService>();
           // services.AddScoped<IService<ConnectionDto>, ConnectionService>();
            services.AddScoped<IService<ExchangeDto>, ExchangeService>();
            services.AddScoped<IService<MessageDto>, MessageService>();
            services.AddScoped<IService<TalentDto>, TalentService>();
            services.AddScoped<IService<TalentRequestDto>, TalentRequestService>();
            services.AddScoped<IService<UserDto>, UserService>();
            
            services.AddAutoMapper(typeof(MyMapper));


            return services;
        }
    }
}
