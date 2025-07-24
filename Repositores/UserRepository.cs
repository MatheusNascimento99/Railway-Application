using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ProjTask.Controllers;
using ProjTask.Enum;
using ProjTask.UserDTOs;
using Microsoft.AspNetCore.JsonPatch;
using BCrypt.Net;


namespace ProjTask.Repositores
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;

        public UserRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            EnsureTableExist();
        }

        private void EnsureTableExist()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            var command = new SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
            BEGIN
                CREATE TABLE Users(
                Id UNIQUEIDENTIFIER PRIMARY KEY,
                Name NVARCHAR (255) NOT NULL,
                Email NVARCHAR (255) NOT NULL UNIQUE,
                Password NVARCHAR (255) NOT NULL,
                Role INT NOT NULL,
                )
            END", connection);
            command.ExecuteNonQuery();
        }


        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT Id, Name, Email, Password, Role FROM Users WHERE Email = @Email", connection);
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
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM Users", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Role = (RoleType)reader.GetInt32(reader.GetOrdinal("Role")),
                });
            }
            return users;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM Users WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (reader.Read())
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
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            var command = new SqlCommand(
                "INSERT INTO Users (Id, Name, Email, Password, Role) VALUES (@Id, @Name, @Email, @Password, @Role)",
            connection);

            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@Name", user.Name);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@Role", user.Role);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<User?> UpdateUserAsync(Guid id, User updateUser)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var command = new SqlCommand(
                @"UPDATE  Users SET Name = @Name, Email = @Email, Password = @Password, Role = @Role WHERE Id = @id"
            , connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", updateUser.Name);
            command.Parameters.AddWithValue("@Email", updateUser.Email);
            command.Parameters.AddWithValue("@Password", updateUser.Password);
            command.Parameters.AddWithValue("@Role", updateUser.Role);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return null;
            }

            var selectCommand = new SqlCommand("SELECT * FROM Users WHERE Id = @id", connection);
            selectCommand.Parameters.AddWithValue("@id", id);
            using var reader = selectCommand.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Password = reader.GetString(reader.GetOrdinal("Password")),
                    Role = (RoleType)reader.GetInt32(reader.GetOrdinal("Role")),
                };
            }

            return null;
        }


        //TODO
        // public async Task<User?> PatchUserAsync(Guid id, JsonPatchDocument<User> patchDoc)
        // {
        //     using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        //     await connection.OpenAsync();

        //     var command = new SqlCommand("SELECT * FROM Users WHERE id=@id", connection);
        //     command.Parameters.AddWithValue("@Id", id);

        //     using var reader = await command.ExecuteReaderAsync();

        //     if (!reader.HasRows)
        //         return null;

        //     User user = new User();
        //     while (await reader.ReadAsync())
        //     {
        //         user.Id = reader.GetGuid(reader.GetOrdinal("Id"));
        //         user.Name = reader.GetString(reader.GetOrdinal("Name"));
        //         user.Email = reader.GetString(reader.GetOrdinal("Email"));
        //         user.Password = reader.GetString(reader.GetOrdinal("Password"));
        //         user.Role = (RoleType)reader.GetInt32(reader.GetOrdinal("Role"));
        //     }

        //     reader.Close();

        //     patchDoc.ApplyTo(user);

        //     command = new SqlCommand(
        //        @"UPDATE  Users SET Name = @Name, Email = @Email, Password = @Password, Role = @Role WHERE Id = @id"
        //    , connection);

        //     command.Parameters.AddWithValue("@Id", id);
        //     command.Parameters.AddWithValue("@Name", user.Name);
        //     command.Parameters.AddWithValue("@Email", user.Email);
        //     command.Parameters.AddWithValue("@Password", user.Password);
        //     command.Parameters.AddWithValue("@Role", user.Role);

        //     var rowsAffected = await command.ExecuteNonQueryAsync();
        //     return rowsAffected > 0 ? user : null;
        // }




        public async Task<bool> DeleteUserAsync(Guid id)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var command = new SqlCommand(
                "DELETE FROM Users WHERE Id = @id", connection
            );
            command.Parameters.AddWithValue("@id", id);
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

    }
}