using ABCRetail.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC with views
builder.Services.AddControllersWithViews();

// Register our Azure Storage services as singletons
// so they are reused across requests
builder.Services.AddSingleton<TableStorageService>();
builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<QueueStorageService>();
builder.Services.AddSingleton<FileStorageService>();

var app = builder.Build();

Console.WriteLine("Keep doing your best ");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();











//using ABCRetail.Services;

//var builder = WebApplication.CreateBuilder(args);


//builder.Services.AddControllersWithViews();

//// Register our Azure Storage services as singletons
//// so they are reused across requests
//builder.Services.AddSingleton<TableStorageService>();
//builder.Services.AddSingleton<BlobStorageService>();

//var app = builder.Build();

//Console.WriteLine("Keep doing your best ");


//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();
