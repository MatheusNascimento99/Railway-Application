using Microsoft.AspNetCore.Mvc;
using ProjTask.Repositores;
using ProjTask.DTOs.TaskItemDtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProjTask.TaskItemDtos;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ProjTask.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repository;

        public TasksController(ITaskRepository repository)
        {
            _repository = repository;
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _repository.GetAllTasksAsync();
            return Ok(tasks);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var task = await _repository.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound(new { message = "Tarefa não encontrada, verifique Id." });
            }
            return Ok(task);

        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskItemCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                UserId = 5
            };

            await _repository.CreateTaskAsync(task);
            return Ok(task);
        }

        [Authorize(Roles = "User")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskAsync(Guid id)
        {
            var deleted = await _repository.DeleteTaskAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Tarefa não encontrada, verifique o Id." });
            }
            else
            {
                return Ok(new { message = "Tarefa deletada com sucesso!." });
            }
        }

        [Authorize(Roles = "User")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskAsync(Guid id, TaskItemUpdateDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority
            };

            var update = await _repository.UpdateTaskAsync(id, task);
            if (update == null)
            {
                return NotFound("Tarefa não atualizada, tente novamente!");
            }
            return Ok(update);
        }
    }
}
