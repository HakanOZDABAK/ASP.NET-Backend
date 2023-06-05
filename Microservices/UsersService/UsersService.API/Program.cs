using UsersService.Application;
using Common.Middlewares;
using Microsoft.AspNetCore.Http.Features;
using UsersService.API.Extensions;
using UsersService.Infrastructure.Persistence;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .Build();

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
  options.InvalidModelStateResponseFactory = actionContext =>
  {
    return Common.Wrappers.Response<string>.ModelValidationErrorResponse(actionContext);
  };
});

builder.Services.AddApplicationLayer(config);
builder.Services.AddPersistenceInfrastructure(config);
builder.Services.AddSwaggerExtension();
builder.Services.AddRulesEngineExtension();

builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(
    builder =>
    {
      builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.Configure<FormOptions>(o =>
{
  o.ValueLengthLimit = int.MaxValue;
  o.MultipartBodyLengthLimit = int.MaxValue;
  o.MemoryBufferThreshold = int.MaxValue;
});

builder.Services.AddHealthChecks();

builder.Services.AddMassTransit(o =>
{
    o.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
      {
          h.Username("admin");
          h.Password("admin");
      });

        cfg.UseNewtonsoftJsonSerializer();
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddOptions<MassTransitHostOptions>().Configure(options =>
{
    options.WaitUntilStarted = true;
    options.StartTimeout = TimeSpan.FromSeconds(10);
    options.StopTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var services = scope.ServiceProvider;

  
}

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseCors();
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHealthChecks("/health");

//var folderName = Path.Combine("Resources", "Images");
//var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
//if (!Directory.Exists(pathToSave))
//  Directory.CreateDirectory(pathToSave);

app.UseDefaultFiles();
app.UseStaticFiles();
//app.UseStaticFiles(new StaticFileOptions()
//{
//  FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Resources")),
//  RequestPath = new PathString("/Resources")
//});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

