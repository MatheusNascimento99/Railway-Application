using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ProjTask.Enum;

namespace ProjTask.UserDTOs
{
    public class CreateUserDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(9)]
        public string Password { get; set; }

        [Range(1, 2, ErrorMessage = "Role inválido. Somente valores 1 (Admin) ou 2 (User) são permitidos.")]
        public RoleType Role { get; set; }
    }
}