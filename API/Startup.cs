using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace API
{
    public class Startup
    {        
        private readonly IConfiguration _config;
        
        public Startup(IConfiguration config)
        {
            _config = config;
                           
        }        

    // This method gets called by the runtime. Use this method to add services to the container.
    //DI container, we can add classes or services here if we want them to be availabe for the entire app
        public void ConfigureServices(IServiceCollection services)
        {
          services.AddDbContext<DataContext>(options =>
          {
              options.UseSqlite(_config.GetConnectionString("DefaultConnection"));
          });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });
            });
        }

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//As we make reuest from browser to controller end point, the request goes through series of middlewares
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
            }

        //http requests will be redirected to https
            app.UseHttpsRedirection();

        //To setup routing mechanism
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}