using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

       

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);     
        }

        public async Task<AppUser> GetUserByUserNameAsync(string username)
        {
            return await _context.Users
            .Include(x=>x.Photos)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
            .Include(x=> x.Photos) 
            .IgnoreQueryFilters()
            .ToListAsync();
        }


        public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(x => x.UserName != userParams.CurrentUserName);
            query = query.Where(x => x.Gender == userParams.Gender);

            var minDOB = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDOB = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
            
            query = query.Where(x => x.DateOfBirth >= minDOB && x.DateOfBirth <= maxDOB);
           
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u=>u.Created),
                _ => query.OrderByDescending(u=>u.LastActive)
            };

            return await PagedList<MemberDTO>.CreateAsync(
               query.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider).AsNoTracking(), 
               userParams.PageNumber, userParams.PageSize);            
        }

         public async Task<MemberDTO> GetMemberAsync(string username, bool isCurrentUser)
        {
            //  return await _context.Users.Where(x=>x.UserName == username)
            //  .Select(user => new MemberDTO
            //  {
            //     Id = user.Id,
            //     UserName = user.UserName
            //  }).SingleOrDefaultAsync();

            var  query = _context.Users.Where(x=>x.UserName == username)            
            .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider);
            
            if(isCurrentUser) 
            {
                query = query.IgnoreQueryFilters();   
            }         
            return await query.SingleOrDefaultAsync();
        }


        // public async Task<bool> SaveUserAsync()
        // {
        //     return await _context.SaveChangesAsync() > 0 ;
        // }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
                .Where(x=>x.UserName == username)
                .Select(x=>x.Gender).FirstOrDefaultAsync();
        }

        public async Task<AppUser> GetUserByPhotoIdAsync(int photoId)
        {
            var photo = await _context.Photos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x=>x.Id == photoId); 
            
            return await _context.Users  
            .Include(x => x.Photos)          
            .Where(x => x.Photos.Contains(photo))
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync();

            // return await _context.Users
            // .Include(p => p.Photos)
            // .IgnoreQueryFilters()
            // .Where(p => p.Photos.Any(p => p.Id == photoId))
            // .FirstOrDefaultAsync();

        }

    }
}