using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using ProjTask.Controllers;
using ProjTask.Enum;

namespace ProjTask.Repositores
{
    public class TaskRepository : ITaskRepository
    {
        private readonly IConfiguration _configuration;

        public TaskRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            EnsureTableExist();
        }

        private void EnsureTableExist() //criando table caso nao exista
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            var command = new SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Tasks' AND xtype='U')
            BEGIN
                CREATE TABLE Tasks(
                Id UNIQUEIDENTIFIER PRIMARY KEY,
                Title NVARCHAR (255) NOT NULL,
                Description NVARCHAR (MAX) NOT NULL,
                Status INT NOT NULL,
                Priority INT NOT NULL,
                CreatedAt DATETIME NOT NULL,
                CompletedAt DATETIME NULL,
                UserId INT NOT NULL
                )
                END", connection);
            command.ExecuteNonQuery();
        }


        public async Task CreateTaskAsync(TaskItem task) //populando o banco de dados
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            if (task.Id == Guid.Empty)
            {
                task.Id = Guid.NewGuid();
            }
            var command = new SqlCommand
            ("INSERT INTO Tasks (Id, Title, Description, Status, Priority, CreatedAt, CompletedAt, UserId) VALUES (@Id, @Title, @Description, @Status, @Priority, @CreatedAt, @CompletedAt, @UserId)",
            connection);

            command.Parameters.AddWithValue("@Id", task.Id);
            command.Parameters.AddWithValue("@Title", task.Title);
            command.Parameters.AddWithValue("@Description", task.Description);
            command.Parameters.AddWithValue("@Status", task.Status);
            command.Parameters.AddWithValue("@Priority", task.Priority);
            command.Parameters.AddWithValue("@CreatedAt", task.CreatedAt);
            command.Parameters.AddWithValue("@CompletedAt", task.CompletedAt);
            command.Parameters.AddWithValue("@UserId", task.UserId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            var tasks = new List<TaskItem>();
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM Tasks", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                tasks.Add(new TaskItem
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    Status = (StatusType)reader.GetInt32(reader.GetOrdinal("Status")),
                    Priority = (PriorityType)reader.GetInt32(reader.GetOrdinal("Priority")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    CompletedAt = reader.IsDBNull(reader.GetOrdinal("CompletedAt"))
                    ? DateTime.UtcNow
                    : reader.GetDateTime(reader.GetOrdinal("CompletedAt")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
                });
            }
            return tasks;
        }

        public async Task<TaskItem> GetTaskByIdAsync(Guid Id)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM Tasks WHERE Id=@id", connection);
            command.Parameters.AddWithValue("@id", Id);
            using var reader = await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                return new TaskItem
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    Status = (StatusType)reader.GetInt32(reader.GetOrdinal("Status")),
                    Priority = (PriorityType)reader.GetInt32(reader.GetOrdinal("Priority")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    CompletedAt = reader.IsDBNull(reader.GetOrdinal("CompletedAt"))
                    ? DateTime.UtcNow
                    : reader.GetDateTime(reader.GetOrdinal("CompletedAt")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
                };
            }
            return null;
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("DELETE FROM Tasks WHERE id=@id", connection);
            command.Parameters.AddWithValue("@id", id);
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<TaskItem> UpdateTaskAsync(Guid id, TaskItem updatedTask)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var command = new SqlCommand(@"UPDATE Tasks SET Title = @Title, Description = @Description, Status = @Status, Priority = @Priority WHERE Id=@id", connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Title", updatedTask.Title);
            command.Parameters.AddWithValue("@Description", updatedTask.Description);
            command.Parameters.AddWithValue("@Status", updatedTask.Status);
            command.Parameters.AddWithValue("@Priority", updatedTask.Priority);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return null;
            }

            var selectCommand = new SqlCommand("SELECT * FROM Tasks WHERE Id = @Id", connection);
            selectCommand.Parameters.AddWithValue("@Id", id);
            using var reader = await selectCommand.ExecuteReaderAsync();
            if (reader.Read())
            {
                return new TaskItem
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    Status = (StatusType)reader.GetInt32(reader.GetOrdinal("Status")),
                    Priority = (PriorityType)reader.GetInt32(reader.GetOrdinal("Priority")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    CompletedAt = reader.IsDBNull(reader.GetOrdinal("CompletedAt"))
                    ? DateTime.UtcNow
                    : reader.GetDateTime(reader.GetOrdinal("CompletedAt")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
                };
            }
            return null;
        }
    }


}