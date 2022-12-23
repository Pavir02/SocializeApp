using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IUserRepository
    {
       public Task<bool> SaveUserAsync();
       public Task<IEnumerable<AppUser>> GetUsersAsync();
       public Task<AppUser> GetUserByIdAsync(int id);
       public Task<AppUser> GetUserByUserNameAsync(string username);
       public void Update(AppUser user);

       public Task<IEnumerable<MemberDTO>> GetMembersAsync();
       public Task<MemberDTO> GetMemberAsync(string username);

    }
}