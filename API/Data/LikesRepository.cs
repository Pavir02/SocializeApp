using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;

        public LikesRepository(DataContext context)
        {
            _context = context;
            
        }

        //Get the UserLike entity
        public async Task<UserLike> GetLike(int sourceUserId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId,targetUserId);
        }


       // Get either the liked users or the users who have liked based on the predicate 
        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = _context.Users.OrderBy(u=>u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            if(likesParams.Predicate=="liked")
            {
                likes = likes.Where(l=>l.SourceUserId == likesParams.UserId);
                users = likes.Select(l=>l.TargetUser);
            }

            if(likesParams.Predicate=="likedBy")
            {
                likes = likes.Where(l=>l.TargetUserId == likesParams.UserId);
                users = likes.Select(l=>l.SourceUser);
            }

            var likedUsers = users.Select(u => new LikeDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Age = u.DateOfBirth.CalculateAge(),
                KnownAs = u.KnownAs,
                City = u.City,
                PhotoUrl = u.Photos.FirstOrDefault(p=>p.IsMain).Url                
            });


            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }


    // Allows to check if the user has been already liked by the other users 
        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(like => like.Id == userId);
        }
    }
}