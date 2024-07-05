using Azure.Storage.Blobs;
using AzureImageSearch.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc();
builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetValue<string>("ConnectionStrings:BlobStorage")));
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DbImageContext>(options =>
{
    options.UseSqlite("DataSource=images.db");
    SQLitePCL.Batteries.Init();
});
var app = builder.Build();

app.MapControllerRoute(name: "default", pattern: "{controller=Image}/{action=Index}");
app.UseStaticFiles();
app.Run();
