using Core.Contracts;

using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Common;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Persistence;

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var host          = builder.Host;
var configuration = builder.Configuration;
var services      = builder.Services;

services.AddCors();
services.AddControllers();

services.AddKeycloakWebApiAuthentication(builder.Configuration);

services
    .AddAuthorization()
    .AddKeycloakAuthorization(configuration)
    .AddAuthorizationBuilder()
    .AddPolicy("AdminUser", builder =>
    {
        builder.RequireRealmRoles("admin"); // Realm role is fetched from token (realm_access.roles)
    })
    .AddPolicy("NormalUser", builder =>
    {
        builder.RequireRealmRoles("user");
    })
    ;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    var keycloakOptions = configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>()!;
    var realmBaseUrl = $"{keycloakOptions.AuthServerUrl!.TrimEnd('/')}/realms/{keycloakOptions.Realm}";

    c.AddSecurityDefinition(
        "oidc",
        new OpenApiSecurityScheme
        {
            Name  = "oauth2",
            Type  = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{realmBaseUrl}/protocol/openid-connect/auth"),
                    TokenUrl         = new Uri($"{realmBaseUrl}/protocol/openid-connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "Standard OpenID scope" },
                        { "profile", "User profile" },
                        { "email", "User email" },
                        { "roles", "User roles" }
                    }
                }
            }
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "oidc"
                    }
                },
                Array.Empty<string>()
            }
        }
    );

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DemoSyp", Version = "v1" });

    var xmlFile = "WebApi.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var sqLiteBuilder = new SqliteConnectionStringBuilder(connectionString);
sqLiteBuilder.DataSource = Path.GetFullPath(
    Path.Combine(
        AppDomain.CurrentDomain.GetData(
            "DataDirectory") as string ?? AppDomain.CurrentDomain.BaseDirectory,
        sqLiteBuilder.DataSource
    ));
connectionString = sqLiteBuilder.ToString();

builder.Services
    .AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString))
    .AddScoped<IUnitOfWork, UnitOfWork>()
    .AddTransient<IMDemoRepository, MDemoRepository>()
    .AddTransient<IImportService, ImportService>()
    ;

var app = builder.Build();
app.MapControllers();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.PreSerializeFilters.Add((doc, request) =>
        {
            if (request.Host.Value!.Contains(".cloud.htl-leonding.ac.at"))
            {
                doc.Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = $"{request.Scheme}s://{request.Host.Value}/demosypapi" } // Add the custom postfix here
                };
            }
        });
    });
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("mysaga-frontend");
        options.OAuthScopes("openid", "profile", "email", "roles");
        options.OAuthUsePkce();
        options.OAuthAppName("mySaga API");
    });
}

// Add CORS to support Single Page Apps (SPAs)
app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
