using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ProjTask.Enum;

namespace ProjTask.Controllers
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [Range(1,3)]
        public StatusType Status { get; set; }
        [Range(1,3)]
        public PriorityType Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public int UserId { get; set; }
    }
}