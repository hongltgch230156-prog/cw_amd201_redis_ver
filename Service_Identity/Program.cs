using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Service_Identity.Data;
using Service_Identity.Repositories;
using Service_Identity.CQRS.Handler;
using Microsoft.AspNetCore.Builder.Extensions;

namespace LoginRegister
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---------- THÊM CẤU HÌNH CORS (Bắt đầu) ----------
            // 1. Thêm dịch vụ CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.SetIsOriginAllowed(origin => true) // cho local dev
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
            // ---------- THÊM CẤU HÌNH CORS (Kết thúc) ----------


            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<GetUserByFirebaseUidHandler>();
            builder.Services.AddScoped<CreateUserHandler>();
            builder.Services.AddScoped<GetUserByFirebaseUidHandler>(); // Duplicates removed

            // Cấu hình database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            // Firebase Admin SDK
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("Config/firebase.json")
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // ---------- SỬ DỤNG CORS ----------
            app.UseCors("AllowFrontend");
            // ---------------------------------------------

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}