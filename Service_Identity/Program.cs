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

            // Cấu hình database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            // Firebase Admin SDK
            //FirebaseApp.Create(new AppOptions
            //{
            //    Credential = GoogleCredential.FromFile("Config/firebase.json")
            //});

            GoogleCredential credential;
            // Nếu đang chạy Development (Local) và chưa set biến môi trường thì đọc file thủ công
            if (builder.Environment.IsDevelopment() && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")))
            {
                credential = GoogleCredential.FromFile("Config/firebase.json");
            }
            else
            {
                // Chạy trên Render hoặc đã set biến môi trường chuẩn
                credential = GoogleCredential.GetApplicationDefault();
            }

            var path = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            Console.WriteLine($"----> DEBUG: Google Credential Path is: '{path}' <----");

            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                    Console.WriteLine("----> DEBUG: File EXISTS! <----");
                else
                    Console.WriteLine("----> DEBUG: File NOT FOUND! Check Secret Files on Render. <----");
            }

            try
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });
                Console.WriteLine("Firebase initialized successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Firebase initialization failed: " + ex);
            }


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    // Lệnh này sẽ tạo bảng AspNetUsers, Tokens... trên Azure SQL
                    context.Database.Migrate();

                    Console.WriteLine("----> Identity Migrations applied successfully! <----");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"----> ERROR applying Identity migrations: {ex.Message} <----");
                }
            }

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