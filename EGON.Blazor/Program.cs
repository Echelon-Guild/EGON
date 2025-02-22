using EGON.Blazor.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Collections.Generic;
using EGON.Blazor.Services;
using Azure.Data.Tables;

var builder = WebApplication.CreateBuilder(args);

// Protect all Razor pages.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
});
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton(provider =>
{
    return new TableServiceClient(Environment.GetEnvironmentVariable("AZURE_PRIVATE_STORAGE_CONNECTION_STRING"));
});

builder.Services.AddSingleton<StorageService>();

// Configure authentication.
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Discord";
})
.AddCookie(options =>
{
    options.LoginPath = "/login"; // Redirect unauthenticated requests to /login.
})
.AddOAuth("Discord", options =>
{
    options.ClientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID");
    options.ClientSecret = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET");
    options.CallbackPath = new PathString("/signin-discord");

    options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
    options.TokenEndpoint = "https://discord.com/api/oauth2/token";
    options.UserInformationEndpoint = "https://discord.com/api/users/@me";

    options.Scope.Add("identify");
    options.SaveTokens = true;

    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
    options.ClaimActions.MapJsonKey("urn:discord:avatar", "avatar");

    options.Events.OnCreatingTicket = async context =>
    {
        var userInfoResponse = await context.Backchannel.GetAsync(context.Options.UserInformationEndpoint);
        if (userInfoResponse.IsSuccessStatusCode)
        {
            using var userJson = JsonDocument.Parse(await userInfoResponse.Content.ReadAsStringAsync());
            context.RunClaimActions(userJson.RootElement);
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// The /login endpoint triggers the Discord OAuth challenge.
app.MapGet("/login", async (HttpContext context) =>
{
    await context.ChallengeAsync("Discord", new AuthenticationProperties { RedirectUri = "/" });
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
