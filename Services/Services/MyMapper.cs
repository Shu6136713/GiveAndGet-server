using AutoMapper;
using Repositories.Entity;
using Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class MyMapper : Profile
    {
        public MyMapper()
        {
            // user
            // convert from server to client
            CreateMap<User, UserDto>()
                .ForMember(
                    dest => dest.Image,
                    src => src.MapFrom(i => i.ProfileImage != null ? convertToByte(Environment.CurrentDirectory + "/Images/" + i.ProfileImage) : null));
            // convert from client to server
            CreateMap<UserDto, User>()
                .ForMember(
                    dest => dest.ProfileImage,
                    src => src.MapFrom(i => i.File != null ? i.File.FileName : "default_profile_image.png")); // Default profile image

            // others - only from server to client
            CreateMap<Comment, CommentDto>().ReverseMap();
            CreateMap<Connection, ConnectionDto>().ReverseMap();
            CreateMap<Exchange, ExchangeDto>().ReverseMap();
            CreateMap<Message, MessageDto>().ReverseMap();
            CreateMap<Talent, TalentDto>().ReverseMap();
            CreateMap<TalentRequest, TalentRequestDto>().ReverseMap();
        }

        public byte[]? convertToByte(string image)
        {
            if (System.IO.File.Exists(image))
            {
                return System.IO.File.ReadAllBytes(image);
            }
            return null;
        }
    }
}