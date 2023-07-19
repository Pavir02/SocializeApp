using System.Security.Claims;
using API.Data;
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
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _uow;

        public UsersController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
        {
            _uow = uow;
            _photoService = photoService;
            _mapper = mapper;
        }



        // [Authorize(Roles="Admin")]
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
        {
            // IEnumerable<AppUser> users =  await _uow.UserRepository.GetUsersAsync();
            // var usersToReturn =  _mapper.Map<IEnumerable<MemberDTO>>(users);
            // return Ok(usersToReturners);

            var gender = await _uow.UserRepository.GetUserGender(User.GetUserName());
            userParams.CurrentUserName = User.GetUserName();
           
            if(string.IsNullOrEmpty(userParams.Gender))
            {
            userParams.Gender = gender=="female"? "male": "female";
            }

            var users = await _uow.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(users.TotalCount, users.CurrentPage,
            users.PageSize, users.TotalPages));
            return Ok(users);
        }


       // [Authorize(Roles = "Member")]
        [HttpGet("{userName}")]
        public async Task<ActionResult<MemberDTO>> GetUser(string userName)
        {
            // AppUser user = await _uow.UserRepository.GetUserByUserNameAsync(userName);
            // return _mapper.Map<MemberDTO>(user);
            var CurrentUserName =User.GetUserName();
            return await _uow.UserRepository.GetMemberAsync(userName, isCurrentUser:CurrentUserName==userName);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {

            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                IsApproved = false
            };

            //if (user.Photos.Count == 0) photo.IsMain = true;
            user.Photos.Add(photo);

            if (await _uow.Complete())
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
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null)
                currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _uow.Complete()) return NoContent();
            return BadRequest();
        }


        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo!");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _uow.Complete()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}