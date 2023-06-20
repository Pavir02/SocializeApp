using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Services container
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddCors();
    // builder.Services.AddSwaggerGen(c =>
    // {
    //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });
    // });

// Middleware
    var app = builder.Build();
    app.UseMiddleware<ExceptionMiddleware>();

    // if (env.IsDevelopment())
    // {
    //     //app.UseDeveloperExceptionPage();
    //     app.UseSwagger();
    //     app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
    // }
 
    //app.UseHttpsRedirection(); //http requests will be redirected to https
    app.UseRouting(); //To setup routing mechanism
    app.UseCors(x => x.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials()
                     .WithOrigins("https://localhost:4200"));
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<PresenceHub>("hubs/presence"); 
    app.MapHub<MessageHub>("hubs/message");       
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        await context.Database.MigrateAsync();
        //context.Connections.RemoveRange(context.Connections);
        await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
        await Seed.SeedUsers(userManager, roleManager);
    }
    catch(Exception ex)
    {
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
    }

    app.Run();
