using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Web6.Interfaces;
using Web6.Services;
using Web6.Models;
using Serilog;
using Amazon.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);
builder.Host.UseSerilog((c, o) =>
{
  o.ReadFrom.Configuration(c.Configuration);
});

var config = builder.Configuration;
builder.Host.ConfigureServices(services =>
{
  services.AddControllers();

  services.AddCors();

  var opts = builder.Configuration.GetAWSOptions();
  opts.Credentials = new BasicAWSCredentials(builder.Configuration["Aws:AccessKey"], builder.Configuration["Aws:SecretKey"]);
  services.AddDefaultAWSOptions(opts);
  services.AddAWSService<IAmazonS3>();

  services.Configure<S3Config>(config.GetSection("S3"));

  services.AddScoped<IMultiPartUploader, MultiPartUploader>();

  services.AddSwaggerGen();
});

await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}

app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseSerilogRequestLogging();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "Uploader Api");
});

app.MapControllers();

app.MapGet("/", (Func<string>)(() => "Hello World!"));

await app.RunAsync();
