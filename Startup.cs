using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Runtime;
using Amazon.S3;
using Uploader.Models;
using Uploader.Interfaces;
using Uploader.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;

namespace Uploader
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();

      services.AddCors();

      var opts = Configuration.GetAWSOptions();
      opts.Credentials = new BasicAWSCredentials(Configuration["Aws:AccessKey"], Configuration["Aws:SecretKey"]);
      services.AddDefaultAWSOptions(opts);
      services.AddAWSService<IAmazonS3>();

      services.Configure<S3Config>(Configuration.GetSection("S3"));

      services.AddScoped<IMultiPartUploader, MultiPartUploader>();

      services.AddSwaggerGen();
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.EnvironmentName.ToLowerInvariant() == "development")
      {
        app.UseDeveloperExceptionPage();
      }


      app.UseSerilogRequestLogging();

      app.UseRouting();

      app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

      app.UseSwagger();

      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Uploader Api");
      });

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}