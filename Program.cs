using platejury_app.Data;

var credentials = @"platejury-app-firebase-adminsdk-r97ah-bcd639d082.json";
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentials);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton(
    sp =>
    {
        var logger = sp.GetRequiredService<ILogger<PlaylistService>>();
        var clientId = builder.Configuration["PlaylistService:ClientId"] ?? "";
        var clientSecret = builder.Configuration["PlaylistService:ClientSecret"] ?? "";
        var tokenUri = builder.Configuration["PlaylistService:TokenURI"] ?? "";
        var playlistUri = builder.Configuration["PlaylistService:PlaylistURI"] ?? "";
        var playlistId = builder.Configuration["PlaylistService:PlaylistId"] ?? "";
        return new PlaylistService(logger, clientId, clientSecret, tokenUri, playlistUri, playlistId);
    }
);
builder.Services.AddSingleton(
    sp =>
    {
        var logger = sp.GetRequiredService<ILogger<VotingService>>();
        var project = builder.Configuration["VotingService:Project"] ?? "";
        var voteCollection = builder.Configuration["VotingService:Collection"] ?? "";
        var themeCollection = "theme";
        return new VotingService(logger, project, voteCollection, themeCollection);
    }
);
builder.Services.AddSingleton(
    sp =>
    {
        var logger = sp.GetRequiredService<ILogger<HistoryService>>();
        return new HistoryService(logger, sp.GetRequiredService<PlaylistService>(), "platejury-app", "history");
    }
);

builder.WebHost.ConfigureKestrel(options => builder.Configuration.GetSection("Kestrel"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
