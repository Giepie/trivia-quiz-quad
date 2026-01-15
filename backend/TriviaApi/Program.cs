using TriviaApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<TriviaService>();
builder.Services.AddScoped<TriviaService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5500", 
                "http://127.0.0.1:5500",
                "https://zealous-bay-05d98bc03.2.azurestaticapps.net")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    options.AddPolicy("AllowProduction", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowFrontend");
}
else
{
    app.UseCors("AllowProduction");
}

app.UseAuthorization();

app.MapControllers();

app.Run();
