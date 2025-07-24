using System;
using System.Collections.Generic;
using System.Linq;
using ProjTask.Enum;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace ProjTask.DTOs.UserDTOs
{
    public class PatchUserDto
    {
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        [Required]
        public RoleType Role { get; set; }
    }
}