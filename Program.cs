using Microsoft.EntityFrameworkCore;
using Tms.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TmsDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
