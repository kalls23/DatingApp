using AutoMapper;
using DatingApp.API.DTOS;
using DatingApp.API.Models;
using System.Collections.Generic;
using System.Linq;


namespace DatingApp.API.helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDTO>()
                .ForMember(dest => dest.PhotoUrl, opt => 
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
                    .ForMember(dest => dest.Age, opt => opt
                        .MapFrom (src => src.DateOfBirth.CalculateAge()));
                
            CreateMap<User, UserForDetailDTO>()
                .ForMember(dest => dest.PhotoUrl, opt => 
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt
                    .MapFrom (src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoForDetailDTO>();
            CreateMap<UserForUpdateDTO, User>();
            CreateMap<Photo, PhotoForReturnDTO>();
            CreateMap<PhotoForCreationDTO, Photo>();
            CreateMap<UserForRegisterDTO, User>();
            CreateMap<MessageForCreationDTO, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnDTO>()
                .ForMember(m => m.SenderPhotoUrl, opt => opt
                    .MapFrom (u => u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(m => m.RecipientPhotoUrl, opt => opt
                    .MapFrom (u => u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));
        }
     }

 }
