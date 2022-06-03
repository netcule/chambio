using System.Text.Json.Serialization;
using Chambio.Server.Extensions;
using Chambio.Server.Persistence;
using MediatR;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.Services.AddSwaggerGen();

builder.Services.AddNpgsql<ChambioContext>(builder.Configuration
    .GetConnectionString("DefaultConnection"));

builder.Services.AddWorkers(builder.Configuration);

builder.Services.AddMediatR(typeof(Program));

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

WebApplication app = builder.Build();

app.UseCors(b => b.AllowAnyOrigin());

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.Run();
