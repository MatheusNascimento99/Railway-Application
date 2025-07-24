using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjTask.Enum;
using ProjTask.Controllers;
using System.ComponentModel.DataAnnotations;

namespace ProjTask.TaskItemDtos
{
    public class TaskItemUpdateDto
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
    }
}