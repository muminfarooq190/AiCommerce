using EcommerceWeb.Handlers;
using EcommerceWeb.Services;
using EcommerceWeb.Services.Contarcts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.MaxAge = TimeSpan.FromDays(30);
});

builder.Services.AddTransient<ApiAuthHandler>();

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    var api = builder.Configuration.GetSection("Api");
    var url = api.GetValue<string>("Url") ?? throw new ArgumentNullException("Api url is misssting in configration.");
    var timeout = api.GetValue<int>("Timeout", 60);
    client.BaseAddress = new Uri(url);
    client.Timeout = TimeSpan.FromSeconds(timeout);
})
.AddHttpMessageHandler<ApiAuthHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.UseSession();
app.Run();
