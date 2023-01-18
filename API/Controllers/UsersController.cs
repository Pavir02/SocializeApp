using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
      
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;         
        }        
    
      [HttpGet]
      public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
      {
        // IEnumerable<AppUser> users =  await _userRepository.GetUsersAsync();
        // var usersToReturn =  _mapper.Map<IEnumerable<MemberDTO>>(users);
        // return Ok(usersToReturners);
        var users = await _userRepository.GetMembersAsync();
        return Ok(users);
      }

      
      [HttpGet("{userName}")]
      public async Task<ActionResult<MemberDTO>> GetUser(string userName)
      {
        // AppUser user = await _userRepository.GetUserByUserNameAsync(userName);
        // return _mapper.Map<MemberDTO>(user);

        return await _userRepository.GetMemberAsync(userName);
      } 

      [HttpPut]
      public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
      {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userRepository.GetUserByUserNameAsync(username);

        if(user == null) return NotFound();

        _mapper.Map(memberUpdateDTO, user);

        if(await _userRepository.SaveUserAsync()) return NoContent();

        return BadRequest("Failed to update user");
      }
  }
}