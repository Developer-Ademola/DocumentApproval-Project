using DocumentApproval_Api.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using DocumentApproval_Api.Interfaces;
using DocumentApproval_Api.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Configuration;

//StaticLogger.EnsureInitialized();
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
            //.AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
            .AddInMemoryTokenCaches();

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Authority = "https://login.microsoftonline.com/215b7ce2-5263-4593-a622-da030405d151/v2.0";
    options.Audience = "b1576e7e-0705-4345-9371-f4d71a6ec1e1";
    options.RequireHttpsMetadata = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "email",
        RoleClaimType = "roles"
    };
});
// Add services to the container.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddControllers();
builder.Services.AddCosmosDbConfiguration(builder.Configuration);
builder.Services.AddStorageAccountConfiguration(builder.Configuration);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbConfig(builder.Configuration);
//builder.Services.AddCosmosDbConfig(builder.Configuration);
//builder.Services.AddSwaggerConfiguration(builder.Configuration);

//builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddScoped<IStorageLake, StorageLake>();
builder.Services.AddScoped<ICosmosDbService, CosmosDbSercice>();
builder.Services.AddSwaggerGen(
 c =>
 {
     c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "DocSeal", Version = "v1" });
     c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
     {
         Description = "Oauth2.0 which uses AuthorizationCode flow",
         Name = "Oauth2",
         Type = SecuritySchemeType.OAuth2,
         Flows = new OpenApiOAuthFlows
         {
             AuthorizationCode = new OpenApiOAuthFlow
             {
                 AuthorizationUrl = new Uri(builder.Configuration["SwaggerAzureAD:Authorization"]),
                 TokenUrl = new Uri(builder.Configuration["SwaggerAzureAD:TokenUrl"]),
                 Scopes = new Dictionary<string, string>
                {
                    {builder.Configuration["SwaggerAzureAD:Scope"], "Access API as User" }
                }
             }
         }

     });
     c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{Type=ReferenceType.SecurityScheme,Id="oauth2"}
            },
            new[]{ builder.Configuration["SwaggerAzureAD:Scope"] }
        }
    });
 });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId(builder.Configuration["SwaggerAzureAD: ClientId"]);
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dispatch API V1");
        c.RoutePrefix = string.Empty;

    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
