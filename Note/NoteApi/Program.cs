using AutoMapper;
using BusinessLogic.Model;
using BusinessLogic.Model.Profile;
using BusinessLogic.Service.Implementation;
using BusinessLogic.Service.Interface;
using BusinessLogic.Validator;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NoteApi.Middleware;
using Postgres.Repository.Implementation;
using Postgres.Repository.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("appsettings.json");
var configuration = configurationBuilder.Build();
builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomAuthenticationService, CustomAuthenticationService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddMemoryCache();

//validation
builder.Services.AddScoped<IValidator<NoteModel>, NoteValidator>();
builder.Services.AddScoped<IValidator<UserRegisterModel>, UserRegisterValidator>();

builder.Services.AddSingleton(
    provider => new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new NotesModelProfile());
            cfg.AddProfile(new UserModelProfile());
        })
        .CreateMapper());

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(
            new StringEnumConverter(new CamelCaseNamingStrategy()));
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddBasicAuthentication(JwtBearerDefaults.AuthenticationScheme,
        JwtBearerDefaults.AuthenticationScheme,
        o => { });

builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.RoutePrefix = string.Empty;
        config.SwaggerEndpoint("swagger/v1/swagger.json", "Notes API");
    });
// }
app.UseCustomExceptionHandler();
app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
            
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
