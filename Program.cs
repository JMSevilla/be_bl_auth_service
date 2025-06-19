using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Oracle.ManagedDataAccess.Client;
using WTW.AuthenticationService.Infrastructure;
using WTW.AuthenticationService.Infrastructure.AccessTokens;
using WTW.AuthenticationService.Infrastructure.AuthenticationDb;
using WTW.AuthenticationService.OpenAM;
using WTW.AuthenticationService.Tokens;
using WTW.Web.Authentication;
using WTW.Web.Errors;
using WTW.Web.Extensions;
using WTW.Web.Logging;
using WTW.Web.OpenAPI;
using WTW.Web.Serialization;
using WTW.Web.Validation;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
builder.RemoveKestrelResponseHeader();
builder.ConfigureLogging(configuration.GetValue<string>("LocalSeqUrl"));

services.AddHealthChecks();
services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
}).UseSerialization();
services.UseValidation();
if (builder.Environment.EnvironmentName != "prod")
    services.UseSwaggerGen(true);
services.AddAuthServiceAuthentication(
    configuration["AuthenticationToken:Issuer"],
    configuration["AuthenticationToken:Audience"],
    configuration["AuthenticationToken:Key"]);
services.AddAuthorization(options =>
{
    options.AddPolicy("BereavementInitialUser", policy => policy.RequireAuthenticatedUser().RequireRole("BereavementInitialUser"));
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireRole("Member")
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddHttpContextAccessor();
RegisterDependencies(services, configuration);

var app = builder.Build();
RuntimeMetrics.Configure();

#warning TODO: need to do migration outside of the process
using (var scope = app.Services.CreateScope())
{
    var isMigrationEnabled = configuration.GetValue<bool>("IsMigrationEnabled");
    if (isMigrationEnabled)
    {
        var context = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
        context.Database.Migrate();
    }
}

app.UseHealthChecks("/health");
app.UseErrorHandling();
app.UseSwagger();

if (app.Environment.EnvironmentName != "prod")
{
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthenticationService v1");
        config.DisplayRequestDuration();
    });
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<SessionHub>("/sessionhub");
});
app.Run();

void RegisterDependencies(IServiceCollection services, IConfiguration configuration)
{
    services.AddHttpClient(
        "OpenAM",
        o => o.BaseAddress = new Uri(configuration["OpenAM:BaseUrl"]))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = false,
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) =>
            {
                return true;
                // if (isDevelopment) return true;
                // return errors == SslPolicyErrors.None;
            }
        });

    services.AddScoped(s => new OpenAMClient(
        s.GetService<IHttpClientFactory>().CreateClient("OpenAM")));
    services.AddScoped<IOpenAMClient>(sp => sp.GetRequiredService<OpenAMClient>());

    services.AddHttpClient("Mdp", o => o.BaseAddress = new Uri(configuration["Mdp:BaseUrl"]))
        .ConfigureHttpClient(c =>
        {
            c.DefaultRequestHeaders.Add("env", configuration["Mdp:Environment"]);
        });
    services.AddScoped(s => new MdpClient(s.GetService<IHttpClientFactory>().CreateClient("Mdp")));
    services.AddScoped<IMdpClient>(sp => sp.GetRequiredService<MdpClient>());

    services.AddDbContext<MemberDbContext>(options =>
        options.UseOracle(new OracleConnection { ConnectionString = configuration.GetConnectionString("MemberDb-DEVPAWEB"), KeepAlive = true }));
    services.AddDbContext<AuthenticationDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("AuthenticationDb")));
    services.AddScoped<UserRepository>();
    services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<UserRepository>());
    services.AddScoped<RefreshTokenRepository>();
    services.AddScoped<IRefreshTokenRepository>(sp => sp.GetRequiredService<RefreshTokenRepository>());
    services.AddSingleton(configuration.GetSection("AuthenticationToken").Get<AuthenticationSettings>());
    services.AddSingleton<IAccessTokenHelper, AccessTokenHelper>();
    services.AddSingleton<AccessToken>();
    services.AddScoped<IAccessToken>(sp => sp.GetRequiredService<AccessToken>());
    services.AddSingleton<IBereavementToken, BereavementToken>();
    services.AddScoped<RefreshTokenFactory>();
    services.AddScoped<IRefreshTokenFactory>(sp => sp.GetRequiredService<RefreshTokenFactory>());
    services.AddScoped<AuthenticationDbUow>();
    services.AddScoped<IAuthenticationDbUow>(sp => sp.GetRequiredService<AuthenticationDbUow>());

    services.AddScoped<LoggingHandler>();
    services.AddSignalR();
    services.AddSingleton<SessionManager>();
    services.ConfigureAll<HttpClientFactoryOptions>(options =>
    {
        options.HttpMessageHandlerBuilderActions.Add(builder =>
        {
            builder.AdditionalHandlers.Add(builder.Services.GetRequiredService<LoggingHandler>());
        });
    });
}