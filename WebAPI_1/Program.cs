using Microsoft.EntityFrameworkCore;
using WebAPI_1.Data;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:5173") // Allow your frontend origin
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
});



//builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowSpecificOrigin");

app.MapControllers();

app.Run();
