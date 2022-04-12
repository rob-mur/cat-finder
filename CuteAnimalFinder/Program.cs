using Azure.Identity;
using CuteAnimalFinder.Services;
using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// Scoped services that are unique to each user session
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ISources, TwitterService>();
builder.Services.AddScoped<IPrediction, Prediction>();
builder.Services.AddScoped<IComponentStateService, ComponentStateService>();
builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddMediatR(cfg => cfg.AsScoped(), typeof(Program));


if (builder.Environment.IsProduction())
    builder.Configuration.AddAzureKeyVault(new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();