using EBrief.Shared.Data;
using EBrief.Shared.Helpers;
using EBrief.WebClient;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddDbContext<ApplicationDbContext>(builder =>
{
    string dbPath = Path.Combine(FileHelpers.AppDataPath, "EBrief.db");
    builder.UseSqlite($"Filename={dbPath}");
});
builder.Services.AddScoped<CourtListDataAccess>();
await builder.Build().RunAsync();
