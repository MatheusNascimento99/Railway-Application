using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ProjTask.Enum;

namespace ProjTask.UserDTOs
{
    public class UpdateUserDto
    {
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        [Required]
        public RoleType Role { get; set; }

    }
}