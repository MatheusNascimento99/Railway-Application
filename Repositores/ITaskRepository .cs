using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjTask.Controllers;

namespace ProjTask.Repositores
{
    public interface ITaskRepository
    {
        Task<TaskItem> GetTaskByIdAsync(Guid Id);
        Task<IEnumerable<TaskItem>> GetAllTasksAsync();
        Task CreateTaskAsync(TaskItem task);
        Task<TaskItem> UpdateTaskAsync(Guid id, TaskItem updatedTask);
        Task<bool> DeleteTaskAsync(Guid Id);
    }
}