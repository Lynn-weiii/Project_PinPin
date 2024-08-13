using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PinPinServer.Hubs;
using PinPinServer.Models;
using PinPinServer.Services;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 設定配置來源 (如 appsettings.json)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddDbContext<PinPinContext>(options => { options.UseSqlServer(builder.Configuration.GetConnectionString("PinPinSQL")); });
builder.Services.AddScoped<AuthGetuserId>();

var PinPinPolicy = "PinPinPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: PinPinPolicy, policy =>
    {
        policy.WithOrigins("*").WithMethods("*").WithHeaders("*");
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();
//JWT驗證用
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

//JWT驗證用
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

//註冊天氣服務    
builder.Services.AddHttpClient<WeatherService>(client =>
{
    client.BaseAddress = new Uri("https://api.openweathermap.org");
});


builder.Services.AddSingleton<WeatherService>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(WeatherService));
    return new WeatherService(builder.Configuration["AppSettings:WeatherApiKey"], httpClient);
});

//註冊匯率服務
builder.Services.AddHttpClient<ChangeRateService>(client =>
{
    client.BaseAddress = new Uri("https://v6.exchangerate-api.com");
});
builder.Services.AddSingleton<ChangeRateService>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ChangeRateService));
    return new ChangeRateService(builder.Configuration["AppSettings:ExChangeRateApiKey"], httpClient);
});

builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(PinPinPolicy);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/ChatHub");
app.MapControllers();

app.Run();
