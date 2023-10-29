using System;

using CodeChallenge.Data;
using CodeChallenge.Models;
using CodeChallenge.Repositories;
using CodeChallenge.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CodeChallenge.Config
{
    public class App
    {
        public WebApplication Configure(string[] args)
        {
            args ??= Array.Empty<string>();

            var builder = WebApplication.CreateBuilder(args);

            builder.UseEmployeeDB();
            
            AddServices(builder.Services);

            var app = builder.Build();

            var env = builder.Environment;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Retrieve the EmployeeContext defined earlier in the UseEmployeeDB function
                var serviceScopeFactory = (IServiceScopeFactory)app.Services.GetService(typeof(IServiceScopeFactory));

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<EmployeeContext>();
                    SeedEmployeeDB(context);
                }                
            }

            app.UseAuthorization();

            app.MapControllers();

            return app;
        }

        private void AddServices(IServiceCollection services)
        {

            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeeRepository, EmployeeRespository>();
            services.AddScoped<ICompensationRepository, CompensationRepository>();

            services.AddControllers();
        }

        private void SeedEmployeeDB(EmployeeContext context)
        {
            new EmployeeDataSeeder(context).Seed().Wait();
        }
    }
}
