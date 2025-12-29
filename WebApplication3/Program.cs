var builder = WebApplication.CreateBuilder(args);

// Servis ekleme

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// HTTP request pipeline oluþturma
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles(); // <--: index.html'i varsayýlan giriþ sayfasý olarak ayarlar
app.UseStaticFiles();  // <--: wwwroot klasöründeki dosyalarýn sunulmasýný saðlar

app.UseAuthorization();  // <--: Token veya cookie yok, local storage kullanýlýyor. bu yüzden gereksiz
                         // Þu an kullanýlmýyor, JWT token eklersek lazým olacak

app.MapControllers();

app.Run();