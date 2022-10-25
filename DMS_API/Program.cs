using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System.Net;
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

         //option.Events = new JwtBearerEvents
         //{
         //    OnAuthenticationFailed = async (context) =>
         //    {
         //        Console.WriteLine("Printing in the delegate OnAuthFailed");
         //    },
         //    OnChallenge = async (context) =>
         //    {
         //        Console.WriteLine("Printing in the delegate OnChallenge");

         //        // this is a default method
         //        // the response statusCode and headers are set here
         //        context.HandleResponse();

         //        // AuthenticateFailure property contains 
         //        // the details about why the authentication has failed
         //        if (context.AuthenticateFailure != null)
         //        {
         //            context.Response.StatusCode = 401;

         //            // we can write our own custom response content here
         //            await context.HttpContext.Response.WriteAsync("Token Validation Has Failed. Request Access Denied");
         //        }
         //    }
         //};

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

    app.Use(async (context, next) =>
    {
        await next();

        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new ResponseModelView
            {
                Success = false,                
                Message = "User Unauthorized - Token is not valid",
                Data = (int)HttpStatusCode.Unauthorized
            }));
        }
        else if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
        {
            await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new ResponseModelView
            {
                Success = false,
                Message = "User Forbidden - Access Denied",
                Data = (int)HttpStatusCode.Forbidden
            }));
        }
        else if (context.Response.StatusCode == (int)HttpStatusCode.InternalServerError)
        {
            await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new ResponseModelView
            {
                Success = false,
                Message = "Internal Server Error",
                Data = (int)HttpStatusCode.InternalServerError
            }));
        }
    });

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