using Microsoft.AspNetCore.SignalR;
using ChatHub;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("http://localhost:3000", "http://localhost:3001") // Specify the allowed origin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app .UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.UseRouting();
app.UseCors("CorsPolicy");
app.MapHub<ChatHub.ChatHub>("chat-hub");
app.Run();
