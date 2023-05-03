using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class RegisterDTO
    {

        [Required]
        public string UserName { get; set; }

        [Required] public string KnownAs {get;set;}

        [Required] public string Gender {get;set;}

        // If we don't make optional, then it gives the default date and not null
        // If it's not null, then Required will not be executed, 
        // hence make it optional to make required work
        [Required] public DateOnly? DateOfBirth {get;set;} 

        [Required] public string City {get;set;}

        [Required] public string Country {get;set;}

        [Required]
        [StringLength(8,MinimumLength =4)]
        public string Password { get; set; }

    }
}