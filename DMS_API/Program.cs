using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using System.Net;
using System.Text;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

try
{
    #region Logs

    builder.Host.UseSerilog(((ctx, lc) => lc
                .WriteTo.File(
                              path: $@"C:\DMS-Logger\Log.txt",
                              rollingInterval: RollingInterval.Day)));
    #endregion
    Log.Information("API is Starting");

    #region Create Server Folder & QR temp Folder & DOC temp Folder
    string pathDMSserver = Path.Combine(builder.Environment.WebRootPath, "DMSserver");
    if (!Directory.Exists(pathDMSserver))
    {
        Directory.CreateDirectory(pathDMSserver);
        for (int i = 0; i <= 1000; i++)
        { Directory.CreateDirectory($"{pathDMSserver}\\{i}"); }
    }

    string pathNewTempQR = Path.Combine(pathDMSserver, $"QRtemp_{DateTime.Now.ToString("dd-MM-yyyy")}");
    if (!Directory.Exists(pathNewTempQR))
    { Directory.CreateDirectory(pathNewTempQR); }
    string pathOldTempQR = Path.Combine(pathDMSserver, $"QRtemp_{DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy")}");
    if (Directory.Exists(pathOldTempQR))
    { Directory.Delete(pathOldTempQR, true); }

    string pathNewTempDOC = Path.Combine(pathDMSserver, $"DOCtemp_{DateTime.Now.ToString("dd-MM-yyyy")}");
    if (!Directory.Exists(pathNewTempDOC))
    { Directory.CreateDirectory(pathNewTempDOC); }
    string pathOldTempDOC = Path.Combine(pathDMSserver, $"DOCtemp_{DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy")}");
    if (Directory.Exists(pathOldTempDOC))
    { Directory.Delete(pathOldTempDOC, true); }
    #endregion
    #region Cors
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
    #endregion
    #region Authentication
    //builder.Services.AddAuthentication(options =>
    //{
    //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    //})
    // .AddJwtBearer(option =>
    // {
    //     var key = "Qig5PmxqgqBbVDtVYRorpC55wm8w3ZrL";// SecurityService.JwtKey;
    //     var KeyByte = Encoding.ASCII.GetBytes(key);
    //     option.SaveToken = true;
    //     option.TokenValidationParameters = new TokenValidationParameters
    //     {
    //         ValidateIssuer = true,
    //         ValidateAudience = true,
    //         ValidateLifetime = true,
    //         ValidateIssuerSigningKey = true,
    //         ValidAudience = "APIsecurity", //SecurityService.JwtAudience,
    //         ValidIssuer = "APIsecurity", //SecurityService.JwtIssuer,
    //         IssuerSigningKey = new SymmetricSecurityKey(KeyByte)
    //     };
    // });
    #endregion
    #region AddSwaggerGen-Authorization
    //builder.Services.AddSwaggerGen(option =>
    //{
    //    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo DMS_API", Version = "v1" });
    //    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    //    {
    //        In = ParameterLocation.Header,
    //        Description = "Please enter a valid token",
    //        Name = "Authorization",
    //        Type = SecuritySchemeType.Http,
    //        BearerFormat = "JWT",
    //        Scheme = "Bearer"
    //    });
    //    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    //{
    //    {
    //        new OpenApiSecurityScheme
    //        {
    //            Reference = new OpenApiReference
    //            {
    //                Type=ReferenceType.SecurityScheme,
    //                Id="Bearer"
    //            }
    //        },
    //        new string[]{}
    //    }
    //});
    //});
    #endregion
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddAntiforgery(options =>
       {
           options.FormFieldName = "AntiforgeryFieldname";
           options.HeaderName = "X-CSRF-TOKEN-HEADERNAME";
           options.SuppressXFrameOptionsHeader = false;
       });

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

    #region HttpStatusCode
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
                Message = MessageService.MsgDictionary[context.Request.Headers["Lang"].ToString().ToLower()][MessageService.ServiceUnavailable],
                Data = (int)HttpStatusCode.InternalServerError
            }));
        }
    });
    #endregion

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseStaticFiles();

    app.UseDirectoryBrowser();

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