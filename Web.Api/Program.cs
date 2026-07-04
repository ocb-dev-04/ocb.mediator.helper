using Application;
using Scalar.AspNetCore;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi()
    .AddControllers();

builder.Services.AddApplicationServices();

WebApplication app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseRouting();
app.MapControllers();

app.Run();
