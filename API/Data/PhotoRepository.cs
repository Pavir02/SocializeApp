using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;

        public PhotoRepository(DataContext context)
        {
            _context = context;
            
        }
        public async Task<Photo> GetPhotoById(int id)
        {
           return await _context.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.Id == id);       
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            var unapprovedPhotos = await _context.Photos
            .IgnoreQueryFilters()
            .Where(x=> x.IsApproved == false)
            .Select(x => new PhotoForApprovalDto
            {
                Id = x.Id,
                Url = x.Url,
                IsMain = x.IsMain,
                IsApproved = x.IsApproved
            })
            .ToListAsync();
            
            return unapprovedPhotos;
        }

        public async void RemovePhoto(Photo photo)
        {
            //var photo = await _context.Photos.IgnoreQueryFilters().Where(x=> x.Id == id).FirstOrDefaultAsync();
            _context.Photos.Remove(photo);
        }
    }
}