
using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

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
app.UseAuthorization();
app.MapControllers();

app.Run();
