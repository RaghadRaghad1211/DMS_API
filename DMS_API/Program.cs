using DMS_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

try
{
    builder.Host.UseSerilog(((ctx, lc) => lc
                .WriteTo.File(
                              path: $@"{Directory.GetCurrentDirectory()}\wwwroot\Logs\Log.txt",
                              rollingInterval: RollingInterval.Day)));


    Log.Information("API is Starting");




    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
    });

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
     .AddJwtBearer(option =>
     {
         var key = SecurityService.JwtKey;
         var KeyByte = Encoding.ASCII.GetBytes(key);
         option.SaveToken = true;
         option.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuer = true,
             ValidateAudience = true,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,
             ValidAudience = SecurityService.JwtAudience,
             ValidIssuer = SecurityService.JwtIssuer,
             IssuerSigningKey = new SymmetricSecurityKey(KeyByte)
         };
     });

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCors();

    //app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    //
    app.UseAuthentication();

    app.UseAuthorization();

    app.UseStaticFiles();

    //app.UseSerilogRequestLogging();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API is Failed");
}
finally
{
    Log.CloseAndFlush();
}