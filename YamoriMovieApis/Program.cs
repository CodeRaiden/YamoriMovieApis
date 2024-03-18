using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using YamoriMovieApis.Models.Domain;
using YamoriMovieApis.Repositories.Abstract;
using YamoriMovieApis.Repositories.Domain;

var builder = WebApplication.CreateBuilder(args);

//// Adding serilog 
//builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
//{
//    loggerConfiguration
//        .ReadFrom.Configuration(hostingContext.Configuration)
//        .Enrich.FromLogContext()
//        .Enrich.WithMachineName()
//        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
//});

// Add services to the container.

builder.Services.AddControllers();
//1. we register the databaseContext for entity framework
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("conn"), o =>
{
    o.EnableRetryOnFailure();
}));

//2. we register the ApplicationUser for Identity by mapping it to the Microsoft Identity "IdentityRole"
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

//3. we add the Authentication with the help of JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

//4. Adding JWTBearer
.AddJwtBearer(options =>
 {
     options.SaveToken = true;
     options.RequireHttpsMetadata = false;
     options.TokenValidationParameters = new TokenValidationParameters()
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
         ValidAudience = builder.Configuration["JWT:ValidAudience"],
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
     };
 });

// 8. Adding the ITokenService and it's implementation TokenService in the application via program.cs file
builder.Services.AddTransient<ITokenService, TokenService>();

var app = builder.Build();



//5. Configure the HTTP request pipeline.

app.UseHttpsRedirection();

//6. we will define the Cross Origin Resource Sharing Configurations
app.UseCors(options =>
    options.WithOrigins("*").AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
);

//7. Configuring our app to use the defined Authentication and Authorization. Must be added in the order below:
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
