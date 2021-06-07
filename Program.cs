using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Uploader;

var app = WebHost
  .CreateDefaultBuilder(args)
  .ConfigureAppConfiguration(c =>
  {
    c.AddJsonFile("appsettings.Local.json", optional: true);
  })
  .UseSerilog((c, o) => o.ReadFrom.Configuration(c.Configuration))
  .UseStartup<Startup>()
  .Build();

await app.RunAsync();
