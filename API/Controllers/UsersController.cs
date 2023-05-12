using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
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
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
        {
            // IEnumerable<AppUser> users =  await _userRepository.GetUsersAsync();
            // var usersToReturn =  _mapper.Map<IEnumerable<MemberDTO>>(users);
            // return Ok(usersToReturners);

            var CurrentUser = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            userParams.CurrentUserName = CurrentUser.UserName;
           
            if(string.IsNullOrEmpty(userParams.Gender))
            {
            userParams.Gender = CurrentUser.Gender=="female"? "male": "female";
            }

            var users = await _userRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(users.TotalCount, users.CurrentPage,
            users.PageSize, users.TotalPages));
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

            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);

            if (await _userRepository.SaveUserAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0) photo.IsMain = true;
            user.Photos.Add(photo);

            if (await _userRepository.SaveUserAsync())
            {
                //return _mapper.Map<PhotoDTO>(photo); 

                return CreatedAtAction(nameof(GetUser),
                   new { username = user.UserName }, _mapper.Map<PhotoDTO>(photo));
            }
            return BadRequest("Problem adding photo");
        }


        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null)
                currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _userRepository.SaveUserAsync()) return NoContent();
            return BadRequest();

        }


        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo!");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveUserAsync()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}