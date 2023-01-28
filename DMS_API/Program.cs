using DMS_API.ModelsView;
using DMS_API.Services;
using Serilog;
using System.Net;

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
    var direcsQR = Directory.GetDirectories(pathDMSserver, "QRtemp_*", SearchOption.TopDirectoryOnly);
    foreach (var dire in direcsQR)
    {
        FileInfo info = new FileInfo(dire);
        if (info.CreationTime <= DateTime.Now.AddDays(-1))
            Directory.Delete(info.FullName, true);
    }


    string pathNewTempDOC = Path.Combine(pathDMSserver, $"DOCtemp_{DateTime.Now.ToString("dd-MM-yyyy")}");
    if (!Directory.Exists(pathNewTempDOC))
    { Directory.CreateDirectory(pathNewTempDOC); }
    var direcsDOC = Directory.GetDirectories(pathDMSserver, "DOCtemp_*", SearchOption.TopDirectoryOnly);
    foreach (var dire in direcsDOC)
    {
        FileInfo info = new FileInfo(dire);
        if (info.CreationTime <= DateTime.Now.AddDays(-1))
            Directory.Delete(info.FullName, true);
    }
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