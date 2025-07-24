using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using ProjTask.Enum;
using ProjTask.UserDTOs;
using BCrypt.Net;
using ProjTask.Controllers;
using Microsoft.AspNetCore.JsonPatch;

namespace ProjTask.Repositores
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
            EnsureTableExist();
        }

        private void EnsureTableExist()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS Users (
                    Id UUID PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Email VARCHAR(255) NOT NULL UNIQUE,
                    Password VARCHAR(255) NOT NULL,
                    Role INT NOT NULL
                )", connection);

            command.ExecuteNonQuery();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                "SELECT Id, Name, Email, Password, Role FROM Users WHERE Email = @Email",
                connection);

            command.Parameters.AddWithValue("@Email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Password = reader.GetString(3),
                    Role = (RoleType)reader.GetInt32(4)
                };
            }

            return null;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand("SELECT Id, Name, Email, Role FROM Users", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Role = (RoleType)reader.GetInt32(reader.GetOrdinal("Role"))
                });
            }
            return users;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                "SELECT Id, Name, Email, Password, Role FROM Users WHERE Id = @id",
                connection);

            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Password = reader.GetString(reader.GetOrdinal("Password")),
                    Role = (RoleType)reader.GetInt32(reader.GetOrdinal("Role"))
                };
            }
            return null;
        }

        public async Task CreateUserAsync(User user)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            var command = new NpgsqlCommand(
                @"INSERT INTO Users (Id, Name, Email, Password, Role) 
                VALUES (@Id, @Name, @Email, @Password, @Role)",
                connection);

            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@Name", user.Name);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(user.Password));
            command.Parameters.AddWithValue("@Role", (int)user.Role);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<User?> UpdateUserAsync(Guid id, User updateUser)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                @"UPDATE Users 
                SET Name = @Name, 
                    Email = @Email, 
                    Password = @Password, 
                    Role = @Role 
                WHERE Id = @Id",
                connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", updateUser.Name);
            command.Parameters.AddWithValue("@Email", updateUser.Email);
            command.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(updateUser.Password));
            command.Parameters.AddWithValue("@Role", (int)updateUser.Role);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return null;
            }

            return await GetUserByIdAsync(id);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                "DELETE FROM Users WHERE Id = @id",
                connection);

            command.Parameters.AddWithValue("@id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Implementação do PatchUserAsync (descomentada e adaptada)
        public async Task<User?> PatchUserAsync(Guid id, JsonPatchDocument<User> patchDoc)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var existingUser = await GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return null;
            }

            patchDoc.ApplyTo(existingUser);

            var command = new NpgsqlCommand(
                @"UPDATE Users 
                SET Name = @Name, 
                    Email = @Email, 
                    Password = @Password, 
                    Role = @Role 
                WHERE Id = @Id",
                connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", existingUser.Name);
            command.Parameters.AddWithValue("@Email", existingUser.Email);
            command.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(existingUser.Password));
            command.Parameters.AddWithValue("@Role", (int)existingUser.Role);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0 ? existingUser : null;
        }
    }
}