using BlazorWebApp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BlazorWebApp.Client.Pages;
using BlazorWebApp.Components;
using BlazorWebApp.Endpoints;
using Data;
using Data.Models.Interfaces;
using System.IO;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddOptions<BlogApiJsonDirectAccessSetting>().Configure(
    options =>
    {
        options.DataPath = $@"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}";
        options.BlogPostsFolder = "Blogposts";
        options.TagsFolder = "Tags";
        options.CategoriesFolder = "Categories";
        options.CommentsFolder = "Comments";
    }
);
builder.Services.AddScoped<IBlogApi, BlogApiJsonDirectAccess>();

builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Authority"] ?? "";
    options.ClientId = builder.Configuration["Auth0:ClientId"] ?? "";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebApp.Client._Imports).Assembly)
    .AddAdditionalAssemblies(typeof(SharedComponents.Pages.Home).Assembly);

app.MapBlogPostAPI();
app.MapCategoryApi();
app.MapTagApi();
app.MapCommentApi();

app.MapGet("account/login", async (string returnUrl, HttpContext context) =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
        .WithRedirectUri(returnUrl)
        .Build();
    
    await context.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("authentication/logout", async (HttpContext context) =>
{
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
        .WithRedirectUri("/")
        .Build();
    
    await context.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.Run();