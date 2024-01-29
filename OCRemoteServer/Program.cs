using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlazorStrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OCRemoteServer;
using OCRemoteServer.Controller;





var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<RazorPagesOptions>(options => options.RootDirectory = "/Pages");
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorStrap();
builder.WebHost.UseKestrel(x => x.ListenAnyIP(125));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
