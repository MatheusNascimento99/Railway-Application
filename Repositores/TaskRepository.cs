using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using ProjTask.Controllers;
using ProjTask.Enum;

namespace ProjTask.Repositores
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;

        public TaskRepository(string connectionString)
        {
            _connectionString = connectionString;
            EnsureTableExist();
        }

        private void EnsureTableExist()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var command = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id UUID PRIMARY KEY,
                    Title VARCHAR(255) NOT NULL,
                    Description TEXT NOT NULL,
                    Status INT NOT NULL,
                    Priority INT NOT NULL,
                    CreatedAt TIMESTAMP NOT NULL,
                    CompletedAt TIMESTAMP NULL,
                    UserId INT NOT NULL
                )", connection);

            command.ExecuteNonQuery();
        }

        public async Task CreateTaskAsync(TaskItem task)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            if (task.Id == Guid.Empty)
            {
                task.Id = Guid.NewGuid();
            }

            var command = new NpgsqlCommand(
                @"INSERT INTO Tasks 
                (Id, Title, Description, Status, Priority, CreatedAt, CompletedAt, UserId) 
                VALUES 
                (@Id, @Title, @Description, @Status, @Priority, @CreatedAt, @CompletedAt, @UserId)",
                connection);

            command.Parameters.AddWithValue("@Id", task.Id);
            command.Parameters.AddWithValue("@Title", task.Title);
            command.Parameters.AddWithValue("@Description", task.Description);
            command.Parameters.AddWithValue("@Status", (int)task.Status);
            command.Parameters.AddWithValue("@Priority", (int)task.Priority);
            command.Parameters.AddWithValue("@CreatedAt", task.CreatedAt);
            command.Parameters.AddWithValue("@CompletedAt",
                task.CompletedAt == DateTime.MinValue ? DBNull.Value : (object)task.CompletedAt);
            command.Parameters.AddWithValue("@UserId", task.UserId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            var tasks = new List<TaskItem>();
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand("SELECT * FROM Tasks", connection);

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
                    ? DateTime.MinValue
                        : reader.GetDateTime(reader.GetOrdinal("CompletedAt")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
                });
            }
            return tasks;
        }

        public async Task<TaskItem> GetTaskByIdAsync(Guid id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand("SELECT * FROM Tasks WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

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
                        ? DateTime.MinValue
                        : reader.GetDateTime(reader.GetOrdinal("CompletedAt")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
                };
            }
            return null;
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand("DELETE FROM Tasks WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<TaskItem> UpdateTaskAsync(Guid id, TaskItem updatedTask)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(@"
                UPDATE Tasks 
                SET 
                    Title = @Title, 
                    Description = @Description, 
                    Status = @Status, 
                    Priority = @Priority,
                    CompletedAt = @CompletedAt
                WHERE Id = @Id", connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Title", updatedTask.Title);
            command.Parameters.AddWithValue("@Description", updatedTask.Description);
            command.Parameters.AddWithValue("@Status", (int)updatedTask.Status);
            command.Parameters.AddWithValue("@Priority", (int)updatedTask.Priority);
            command.Parameters.AddWithValue("@CompletedAt",
                updatedTask.CompletedAt == DateTime.MinValue ? DBNull.Value : (object)updatedTask.CompletedAt);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return null;
            }

            return await GetTaskByIdAsync(id);
        }
    }
}