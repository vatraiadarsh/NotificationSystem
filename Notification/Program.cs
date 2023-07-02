using Microsoft.EntityFrameworkCore;
using Notification.DTO;

namespace Notification
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<NotificationDbContext>(options =>
                options.UseSqlServer(connectionString));

           

            var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMQConfigDTO>();
            builder.Services.AddSingleton(rabbitMQConfig);
            builder.Services.AddSingleton<RabbitMQService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            using (var app = builder.Build())
            {
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseAuthorization();

                app.MapControllers();

                var serviceProvider = app.Services;
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                    var progressLock = new object();
                    var progressBarWidth = 69;

                    var progress = new Progress<int>(percent =>
                    {
                        lock (progressLock)
                        {
                            Console.CursorVisible = false;
                            Console.Write("\r[");
                            int completedBars = (int)((double)percent / 100 * progressBarWidth);
                            int remainingBars = progressBarWidth - completedBars;
                            string progressBar = new string('#', completedBars) + new string('-', remainingBars);
                            Console.Write(progressBar);
                            Console.Write($"] {percent}%");
                        }
                    });

                    if (dbContext.HasRequiredTable())
                    {
                        dbContext.SeedData(progress).GetAwaiter().GetResult();
                        Console.WriteLine("\nSeed data inserted into the database.");
                    }
                    else
                    {
                        Console.WriteLine("\nUser table not found.");
                    }
                }


                app.Run();
            }
        }
    }
}
