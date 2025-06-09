using ToDoApp.Services;
using MongoDB.Driver;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Servisleri ekle
builder.Services.AddControllers();
builder.Services.AddSingleton<TaskService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDo API", Version = "v1" });
});

var app = builder.Build();

// Mongo bağlantı testi
try
{
    var client = new MongoClient(builder.Configuration["MongoDB:ConnectionString"]);
    var dbs = client.ListDatabaseNames().ToList();
    Console.WriteLine("✅ MongoDB bağlantısı başarılı.");
}
catch (Exception ex)
{
    Console.WriteLine("❌ MongoDB bağlantı hatası:");
    Console.WriteLine(ex.Message);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
