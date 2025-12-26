
using System.Text;
using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var dbConnectionString= builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));

//Registar repository y automapper
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddAutoMapper(configuration =>
{
    // Escanea todos los perfiles en el ensamblado de Program
    configuration.AddMaps(typeof(Program).Assembly);
});

//Manejo de autorizacion en apis
var secretKey= builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if(string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("SecretKey no esta configurada");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken=true;
    options.TokenValidationParameters= new TokenValidationParameters
    {
        ValidateIssuerSigningKey=true,
        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer =false,
        ValidateAudience=false
    };
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
//Manejo de swagger
builder.Services.AddSwaggerGen(options =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization usando Bearer",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});
//Manejo de CORS
builder.Services.AddCors( options =>
{
    options.AddPolicy(PoliceNames.AllowSpecificOrigin,
    builder =>
    {
        builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(PoliceNames.AllowSpecificOrigin);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();