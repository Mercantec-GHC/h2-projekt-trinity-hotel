using Blazor.Components;
using Microsoft.EntityFrameworkCore.Storage;
using API.Data;
using Blazor.Services;
using DomainModels;
using Microsoft.EntityFrameworkCore;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DefaultConnection");

        // Configure HotelContext
        builder.Services.AddDbContext<HotelContext>(options =>
        options.UseNpgsql(connectionString));

        // AuthenticationService Registration
        builder.Services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {

            });

        // Register AppState
        builder.Services.AddScoped<AppState>();

        builder.Services.AddScoped<MailServices>();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddHttpClient("API", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7207/");
        });

        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();

    }
}