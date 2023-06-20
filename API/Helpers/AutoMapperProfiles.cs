using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {

            //configuring automapper to shape the data as we want/ map specific property
            //map the Url of the main photo form the AppUser to PhotoUrl in MemberDTO
            CreateMap<AppUser,MemberDTO>()
            .ForMember(dest => dest.PhotoUrl,   
                    options => options.MapFrom(source => source.Photos.FirstOrDefault(x => x.IsMain).Url))
            .ForMember(dest => dest.Age,
                    options => options.MapFrom(source => source.DateOfBirth.CalculateAge())) ;

            CreateMap<Photo, PhotoDTO>();   

            CreateMap<MemberUpdateDTO, AppUser>();  

            CreateMap<RegisterDTO, AppUser>(); 

            CreateMap<Message, MessageDTO>()
            .ForMember(dest => dest.SenderPhotoUrl,
                    options => options.MapFrom(source => 
                                source.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
            .ForMember(dest => dest.RecipientPhototUrl,
                    options => options.MapFrom(source => 
                                source.Recipient.Photos.FirstOrDefault(x=> x.IsMain).Url));    

            CreateMap<DateTime, DateTime>()
            .ConvertUsing(d => DateTime.SpecifyKind(d,DateTimeKind.Utc));

            CreateMap<DateTime?, DateTime?>()
            .ConvertUsing(d => d.HasValue? DateTime.SpecifyKind(d.Value ,DateTimeKind.Utc)
                                         : null);

        }
    }
}