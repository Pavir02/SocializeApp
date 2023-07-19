using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _uow;
        private readonly IPhotoService _photoService;

        public AdminController(UserManager<AppUser> userManager, IUnitOfWork uow, IPhotoService photoService)
        {
            _photoService = photoService;
            _uow = uow;
            _userManager = userManager;            
        }
    
        [Authorize(Policy ="RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
          var users = await _userManager.Users
           .OrderBy(x => x.UserName)
           .Select(u => new 
            {
                u.Id,
                UserName = u.UserName,
                Roles = u.UserRoles.Select(x=> x.Role.Name).ToList()    
            }).ToListAsync();

            return Ok(users);
        }


        [Authorize(Policy ="RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
          if(string.IsNullOrEmpty(roles)) return BadRequest("You must select atleast one role!");
          var selectedRoles = roles.Split(',').ToArray();
            
          var user = await _userManager.FindByNameAsync(username);
          if(user is null) return NotFound();

          var userRoles = await _userManager.GetRolesAsync(user);

          var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
          if(!result.Succeeded) return BadRequest("Failed to add to roles");

          result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
          if(!result.Succeeded) return BadRequest("Failed to remove from roles");

        return Ok(await _userManager.GetRolesAsync(user));

        }

       
        [Authorize(Policy ="ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var photos = await _uow.PhotoRepository.GetUnapprovedPhotos();
            return Ok(photos);
        }

        [Authorize(Policy ="ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoid}")]
        public async Task<ActionResult> ApprovePhoto(int photoid)
        {
            var photo = await _uow.PhotoRepository.GetPhotoById(photoid); 

            if (photo == null) return NotFound("Could not find the photo");            
            if (photo.IsApproved) return BadRequest("The photo is already approved!");

            var user = await _uow.UserRepository.GetUserByPhotoIdAsync(photoid);
            if(user!=null)
            {
                photo.IsApproved = true;

                if (!user.Photos.Any(x => x.IsMain))
                  photo.IsMain = true; // If there is no main already, set the current approved photo as main
            }
            
            if (await _uow.Complete()) return Ok();
            return BadRequest("Problem approving the photo");

        }

        [Authorize(Policy ="ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoid}")]
        public async Task<ActionResult> RejectPhoto(int photoid)
        {
            var photo = await _uow.PhotoRepository.GetPhotoById(photoid);
            if (photo == null) return NotFound("Could not find the photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Result == "ok")
                {
                _uow.PhotoRepository.RemovePhoto(photo);
                }
            }
            else
            {
                _uow.PhotoRepository.RemovePhoto(photo);
            }

            if (await _uow.Complete()) return Ok();
            return BadRequest("Problem rejecting the photo");
        }
    }
}