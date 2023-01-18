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
            .ForMember(memberDto => memberDto.PhotoUrl,   
                options => options.MapFrom(appuser =>
                appuser.Photos.FirstOrDefault(x=>x.IsMain == true).Url))
            .ForMember(memberDto => memberDto.Age,
                options => options.MapFrom(appuser => 
                appuser.DateOfBirth.CalculateAge())) ;

            CreateMap<Photo,PhotoDTO>();   
            CreateMap<MemberUpdateDTO, AppUser>();         
        }
    }
}