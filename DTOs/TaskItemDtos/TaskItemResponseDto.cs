using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ProjTask.Controllers;
using ProjTask.Enum;

namespace ProjTask.DTOs
{
    public class TaskItemResponseDto // para qual verbo ?
    {
        [Required]
        [MinLength(3)]
        public string Title { get; set; }
        [MaxLength(300)]
        public string Description { get; set; }
        [Required]
        public StatusType Status { get; set; }
        [Required]
        public PriorityType Priority { get; set; }
        [Required]
        public string User { get; set; } //tipo string ou User?
    }
}