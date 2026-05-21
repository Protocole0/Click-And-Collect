using ClickAndCollect.DAL;
using ClickAndCollect.Interfaces;
using ClickAndCollect.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

string connectionString = builder.Configuration.GetConnectionString("Default")!;

builder.Services.AddTransient<ICategoryDAL>(_ => new CategoryDAL(connectionString));
builder.Services.AddTransient<IProductDAL>(_ => new ProductDAL(connectionString));
builder.Services.AddTransient<IOrderDAL>(_ => new OrderDAL(connectionString));
builder.Services.AddTransient<IUserDAL>(_ => new UserDAL(connectionString));
builder.Services.AddTransient<IStoreDAL>(_ => new StoreDAL(connectionString));
builder.Services.AddTransient<ITimeSlotDAL>(_ => new TimeSlotDAL(connectionString));
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
