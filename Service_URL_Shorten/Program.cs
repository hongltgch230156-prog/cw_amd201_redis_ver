using Google.Cloud.Firestore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Service_URL_Shorten.Commands;
using Service_URL_Shorten.Data;
using Service_URL_Shorten.Queries;
using StackExchange.Redis;

namespace Service_URL_Shorten
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add MediatR to the container
            builder.Services.AddMediatR(typeof(Program).Assembly);

            builder.Services.AddMemoryCache();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "https://url-shortener-frontend-p2sn.onrender.com", "http://localhost:8085") 
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var redisConnection = builder.Configuration.GetConnectionString("Redis");

            // 2. Cấu hình Redis Cache
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;

                options.InstanceName = "UrlShortener_";
            });


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    context.Database.Migrate();
                    Console.WriteLine("----> Service Migrations applied successfully! <----");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"----> ERROR applying migrations: {ex.Message} <----");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Thay dòng app.UseHttpsRedirection();
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }


            app.UseCors("AllowFrontend");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
