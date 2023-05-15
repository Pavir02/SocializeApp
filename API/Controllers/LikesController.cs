using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController :BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;

        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;            
        }


        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string userName)
        {
            var sourceUserId = User.GetUserId();
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            var likedUser = await _userRepository.GetUserByUserNameAsync(userName);

            if(likedUser == null) return NotFound();
            if(sourceUser.UserName == userName) return BadRequest("You cannot like yourself!");

            var userLike = await _likesRepository.GetLike(sourceUserId, likedUser.Id);
            if(userLike != null) return BadRequest("You have already liked this user!");

            userLike = new UserLike{
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);
            if( await _userRepository.SaveUserAsync()) return Ok();

            return BadRequest("Failed to like the user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesRepository.GetUserLikes(likesParams);
            
            Response.AddPaginationHeader(new PaginationHeader(users.TotalCount, users.CurrentPage, 
                users.PageSize, users.TotalPages));
            
            return Ok(users);
        }       
    }
}