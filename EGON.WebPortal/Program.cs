using Azure.Data.Tables;
using EGON.WebPortal.Services;
using EGON.WebPortal.Services.WoW;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpClient();

builder.Services.AddSingleton(provider =>
{
    return new TableServiceClient(Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING"));
});

builder.Services.AddSingleton<StorageService>();

builder.Services.AddSingleton<BattleNetAuthService>();

builder.Services.AddSingleton<WoWApiService>();

// Authentication

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Discord";
})
.AddCookie()
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
