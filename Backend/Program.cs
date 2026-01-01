using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
        options.JsonSerializerOptions.AllowTrailingCommas = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração de CORS para permitir chamadas do frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();