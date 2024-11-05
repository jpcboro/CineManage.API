using System.Text;
using AutoMapper;
using CineManage;
using CineManage.API.Data;
using CineManage.API.Services;
using CineManage.API.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "CineManage.API", Version = "v1"});
    
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    
    option.AddSecurityRequirement( new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
    
});
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});

var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")!.Split(",");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders(Constants.TotalRecordsCount);
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseSqlServer("name=DefaultConnection", sqlServer => sqlServer.UseNetTopologySuite());
});

builder.Services.AddSingleton<GeometryFactory>(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));

//builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddSingleton(provider => new MapperConfiguration(config =>
{
    var geoFactory = provider.GetRequiredService<GeometryFactory>();
    config.AddProfile(new MapperProfiles(geoFactory));
}).CreateMapper()
);

builder.Services.AddTransient<IFileStorage, AzureFileStorage>();
builder.Services.AddTransient<IUsersService, UsersService>();

builder.Services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<IdentityUser>>();
builder.Services.AddScoped<SignInManager<IdentityUser>>();


builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding
            .UTF8.GetBytes(builder.Configuration["jwtKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.AuthorizationIsAdmin, policy => policy.RequireClaim(Constants.AuthorizationIsAdmin));
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.Map("/mapTest", (app) =>
{
    app.Run(async context =>
    {
        await context.Response.WriteAsync("SHORT CIRCUITED!");
    });
});

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();

app.UseOutputCache();

// app.UseResponseCaching();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();