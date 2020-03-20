using System.Text;
using AutoMapper;
using API.Data;
using API.Data.AuthRepository;
using API.Data.CloudStorage;
using API.Data.ProfileRepository;
using API.Data.QRCodeCreation;
using API.Data.InstitutionRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Adding Controller as service
            services.AddControllers();

            // Adding HttpClient service for fetching external Apis
            services.AddHttpClient();

            // Add Database to the Services
            services.AddDbContext<DataContext>(options =>
            {
                string connectionString = Environment.GetEnvironmentVariable("POSTGRES_DB");
                if (connectionString == null)
                    options.UseNpgsql(Configuration.GetSection("DatabaseConfig")["PostgresDb"]);
                else
                    options.UseNpgsql(connectionString);
            });

            // Add Automapper to map objects of different types
            services.AddAutoMapper();

            // Add Authrepository
            services.AddScoped<IAuthRepository, AuthRepository>();

            // Add Profile
            services.AddScoped<IProfileRepository, ProfileRepository>();

            // Add Google Cloud Storage
            services.AddScoped<ICloudStorage, GoogleCloudStorage>();

            //Adding QRCode Creation to the service
            services.AddScoped<IQRCodeGeneration, QRCodeGeneration>();

            //Adding InstitutionRepository to the service
            services.AddScoped<IInstitutionRepository, InstitutionRepository>();

            // Add Jwt Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                    ValidateIssuer = true,
                    ValidIssuer = Configuration.GetSection("AppSettings:Issuer").Value,
                    ValidateAudience = false
                };
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataContext dataContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Uncomment following line when having https
            // app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "API");
            });

            // Automatic database update
            dataContext.Database.Migrate();

            // Allow Cross origin requests
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            // Use Routing
            app.UseRouting();

            // Use Authentication
            app.UseAuthentication();

            // User authorization
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}