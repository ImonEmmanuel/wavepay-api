using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Paywave.Middleware;
using PaywaveAPICore;
using PaywaveAPICore.Constant;
using PaywaveAPICore.Processor;
using PaywaveAPIData.DataService.Interface;
using PaywaveAPIMongoDataService.DataService;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Processor
builder.Services.AddTransient<AuthProcessor>();


//DataService
builder.Services.AddTransient<IAutheticationDataService, AutheticationDataService>();
builder.Services.AddTransient<ITokenDataService, TokenDataService>();




builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Paywave API", Description = "💲💰 Financial Payment System API", Version = "v1"});
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer Scheme.
                        Enter 'Bearer'[space] and then your token in the text input below.
                        Example: Bearer 12345abcdef ",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"},
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,

                },
                new List<string>()
                }
            });
});

builder.Services.Configure<JwtTokenConfig>(
    builder.Configuration.GetSection("JwtTokenConfig"));



string jwtSecret = builder.Configuration.GetSection("JwtTokenConfig:Secret").Value;
string jwtIssuer = builder.Configuration.GetSection("JwtTokenConfig:Issuer").Value;
string jwtAudience = builder.Configuration.GetSection("JwtTokenConfig:Audience").Value;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience
            };
        }
);


builder.Services.AddAuthentication();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ALLOW",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod()
            .AllowAnyHeader();
        });
});






var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Paywave API");
    });
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Paywave API");
});

app.UseMiddleware<JwtMiddleware>();
app.UseExceptionHandlingMiddleware();

app.UseRouting();
app.UseCors("ALLOW");// Place UseCors after UseRouting and before UseEndpoints

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


app.Run();
