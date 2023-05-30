using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        //private readonly DataContext _context;
        private readonly  ITokenService _tokenService ;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        public AccountController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
        ITokenService tokenService, IMapper mapper )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
            // _context = context;
        }


        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {

            if(await UserExists(registerDTO.UserName)) return BadRequest("Username is already taken") ;

            var user  = _mapper.Map<AppUser>(registerDTO);

            // using var hmac = new HMACSHA512();
           
            user.UserName = registerDTO.UserName.ToLower();
                // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
                // user.PasswordSalt = hmac.Key;          

            // _context.Users.Add(user);
            // await _context.SaveChangesAsync();

            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if(!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if(!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDTO
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Token =  await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                Gender = user.Gender

            };
        }

        private async Task<bool> UserExists(string userName)
        {
            return  await _userManager.Users.AnyAsync(x=> x.UserName.ToLower() == userName.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) 
        {
            var user =  await _userManager.Users
                    .Include(p => p.Photos)
                    .SingleOrDefaultAsync(x=>x.UserName == loginDTO.UserName);

            if (user==null)
            {
                return Unauthorized("Invalid Username");
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
         
            if(!result)
            {
                return Unauthorized("Invalid Password");
            }

            // using var hmac = new HMACSHA512(user.PasswordSalt);
            // //compute the password hash from the password salt stored in db(above line) and the user entered password
            // var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            // //now check if the computedhash and the passwordhash stored in DB are same
            // for(int i=0; i<computedHash.Length; i++)
            // {
            //     if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            // }

            return new UserDTO
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                Gender = user.Gender
            };
        }

    }
}