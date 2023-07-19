using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
    //    public Task<bool> SaveUserAsync();
       public Task<IEnumerable<AppUser>> GetUsersAsync();
       public Task<AppUser> GetUserByIdAsync(int id);
       public Task<AppUser> GetUserByUserNameAsync(string username);
       public void Update(AppUser user);
       public Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams);
       public Task<MemberDTO> GetMemberAsync(string username, bool isCurrentUser);
       public Task<string> GetUserGender(string username);
       public Task<AppUser> GetUserByPhotoIdAsync(int photoId);


    }
}