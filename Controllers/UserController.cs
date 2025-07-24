using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using ProjTask.DTOs.UserDTOs;
using ProjTask.Repositores;
using ProjTask.UserDTOs;
using Microsoft.AspNetCore.JsonPatch;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace ProjTask.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repository;

        public UsersController(IUserRepository repository)
        {
            _repository = repository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _repository.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _repository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado, verifique o id." });
            }
            return Ok(user);
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
            };

            await _repository.CreateUserAsync(user);

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var deleted = await _repository.DeleteUserAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }
            else
            {
                return Ok(new { menssage = "Usuário apagado com sucesso!" });
            }
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, UpdateUserDto dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                Role = dto.Role
            };

            var update = await _repository.UpdateUserAsync(id, user);
            if (update == null)
            {
                return NotFound("Tarefa não atualizada, tente novamente!");
            }
            return Ok(update);
        }

        //TODO
        // [HttpPatch("{id}")]
        // public async Task<IActionResult> PatchUser(Guid id, [FromBody] JsonPatchDocument<User> patchDoc)
        // {

        //     if (patchDoc == null)
        //         return BadRequest("Patch Inválido!");

        //     var updateUser = await _repository.PatchUserAsync(id, patchDoc);

        //     if (updateUser == null)
        //         return NotFound("Usuário não encontrado! Verifique o id.");

        //     return Ok(updateUser);
        // }

    }
}

