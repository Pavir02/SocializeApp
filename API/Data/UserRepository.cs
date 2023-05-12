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
            .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
            .Include(x=> x.Photos) 
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

         public async Task<MemberDTO> GetMemberAsync(string username)
        {
            //  return await _context.Users.Where(x=>x.UserName == username)
            //  .Select(user => new MemberDTO
            //  {
            //     Id = user.Id,
            //     UserName = user.UserName
            //  }).SingleOrDefaultAsync();

            return await _context.Users.Where(x=>x.UserName == username)
            .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }


        public async Task<bool> SaveUserAsync()
        {
            return await _context.SaveChangesAsync() > 0 ;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}