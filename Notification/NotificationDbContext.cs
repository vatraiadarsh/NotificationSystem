using Bogus;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Notification.DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Notification
{
    public class NotificationDbContext : DbContext
    {
        public DbSet<NotificationDTO> Notifications { get; set; }
        public DbSet<UserDTO> Users { get; set; }

        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public async Task SeedData(IProgress<int> progress)
        {
            if (Users.Count() > 1000)
            {
                var users = GenerateUsers(100000, progress);
                await BulkInsertUsers(users,progress);
            }
        }

        public bool HasRequiredTable()
        {
            string tableName = "Users";
            string query = $"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";

            using (var command = Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    return result.HasRows;
                }
            }
        }


        private List<UserDTO> GenerateUsers(int count, IProgress<int> progress)
        {
            var users = new ConcurrentBag<UserDTO>();
            var progressLock = new object();

            Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                var faker = new Faker<UserDTO>()
                   .RuleFor(u => u.Id, f => Guid.NewGuid())
                   .RuleFor(u => u.FirstName, f => f.Person.FirstName)
                   .RuleFor(u => u.LastName, f => f.Person.LastName)
                   .RuleFor(u => u.Email, f => f.Person.Email);

                var user = faker.Generate();
                users.Add(user);

                int completedCount = users.Count;
                int percent = (int)(((double)completedCount / count) * 100);
                progress?.Report(percent);
            });

            return users.ToList();
        }

        private async Task BulkInsertUsers(List<UserDTO> users, IProgress<int> progress)
        {
            var options = new BulkConfig { BatchSize = 1000 };
            await this.BulkInsertAsync(users, options);
            progress?.Report(100);
        }
    }

}