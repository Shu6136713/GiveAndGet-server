using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Entity;
using Repositories.Interfaces;
using Repositories.Repositories;
using Services.Dtos;
using Services.Interfaces;

namespace Services.Services
{
    public static class ExtensionService
    {
        public static IServiceCollection AddServiceExtension(this IServiceCollection services)
        {
            services.AddRepository();
            services.AddScoped<IService<CommentDto>, CommentService>();
            services.AddScoped<IService<ExchangeDto>, ExchangeService>();
            services.AddScoped<IService<MessageDto>, MessageService>();
            services.AddScoped<IService<TalentDto>, TalentService>();
            services.AddScoped<IService<TalentRequestDto>, TalentRequestService>();
            services.AddScoped<IService<UserDto>, UserService>();
            services.AddScoped<IService<TalentUserDto>, TalentUserService>();

            services.AddScoped<ITalentExtensionService, TalentService>();
            services.AddAutoMapper(typeof(MyMapper));

            return services;
        }
    }
}