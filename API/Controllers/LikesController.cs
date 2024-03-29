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
        private readonly IUnitOfWork _uow;

        public LikesController(IUnitOfWork uow)
        {
            _uow = uow;
                       
        }


        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string userName)
        {
            var sourceUserId = User.GetUserId();
            // var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId);
            // var likedUser = await _uow.UserRepository.GetUserByUserNameAsync(userName);
            
            var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId);
            var likedUser = await _uow.UserRepository.GetUserByUserNameAsync(userName);

            if(likedUser == null) return NotFound();
            if(sourceUser.UserName == userName) return BadRequest("You cannot like yourself!");

            var userLike = await _uow.LikesRepository.GetLike(sourceUserId, likedUser.Id);
            if(userLike != null) return BadRequest("You have already liked this user!");

            userLike = new UserLike{
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if( await _uow.Complete()) return Ok();
            return BadRequest("Failed to like the user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _uow.LikesRepository.GetUserLikes(likesParams);
            
            Response.AddPaginationHeader(new PaginationHeader(users.TotalCount, users.CurrentPage, 
                users.PageSize, users.TotalPages));
            
            return Ok(users);
        }       
    }
}