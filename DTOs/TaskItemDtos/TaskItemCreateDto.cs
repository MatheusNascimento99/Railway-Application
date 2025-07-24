using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjTask.Enum;
using System.ComponentModel.DataAnnotations;

namespace ProjTask.DTOs.TaskItemDtos
{
    public class TaskItemCreateDto
    {
        [Required]
        public string Title { get; set; }
        [MaxLength(300)]
        public string Description { get; set; }
        [Range(1, 3, ErrorMessage = "Status inválido. Somente valores 1 (Pending), 2 (InProgress) ou 3 (Completed) são permitidos.")]
        [Required]
        public StatusType Status { get; set; }
        [Range(1, 3, ErrorMessage = "Priority inválido. Somente valores 1 (Low), 2 (Medium) ou 3 (High) são permitidos.")]

        [Required]
        public PriorityType Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public int User { get; set; }
    }
}