using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjTask.Controllers;
using Microsoft.AspNetCore.JsonPatch;

namespace ProjTask.Repositores
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(Guid Id);

        Task<IEnumerable<User>> GetAllUsersAsync();

        Task CreateUserAsync(User user);

        Task<User?> UpdateUserAsync(Guid id, User updateUser);

        // Task<User?> PatchUserAsync(Guid id, JsonPatchDocument <User> patchDoc);

        Task<bool> DeleteUserAsync(Guid Id);

        Task<User?> GetUserByEmailAsync(string email);
    }
}