using APBDTut9API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IApiService, ApiService>();

var app = builder.Build();

app.MapControllers();
app.Run();