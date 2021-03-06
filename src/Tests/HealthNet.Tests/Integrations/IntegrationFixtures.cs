using System;
using System.Collections.Generic;
using System.Net.Http;
using HealthNet.AspNetCore;
using HealthNet.Integrations.Runners;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace HealthNet.Integrations
{
  [TestFixture(typeof(NancyFixturesRunner))]
  [TestFixture(typeof(OwinFixturesRunner))]
  [TestFixture(typeof(WebApiFixturesRunner))]
  [TestFixture(typeof(AspNetCoreFixturesRunner))]
  abstract class IntegrationFixtures<TFixtureRunner> where TFixtureRunner : IFixtureRunner, new()
  {
    [OneTimeSetUp]
    public void SetUp()
    {
      IFixtureRunner runner = new TFixtureRunner();
      using (var server = new TestServer(new WebHostBuilder()
        .ConfigureServices(services =>
        {
          var config = GetConfiguration();
          services.AddTransient(x => CreateCheckers());
          if (config == null)
          {
            services.AddHealthNet<TestHealthNetConfiguration>();
          }
          else
          {
            services.AddTransient<IVersionProvider, AssemblyFileVersionProvider>();
            services.AddTransient(x => config);
            services.AddHealthNet();
          }
         
          ConfigureDependencies(services);
        })
        .Configure(app => runner.Configure(app).Run(async context =>
        {
          context.Response.ContentType = "text/plain";
          await context.Response.WriteAsync("Hello World");
        }))))
      {
        Response = server.CreateClient().GetAsync(Path).Result;

        RawContent = Response.Content.ReadAsStringAsync().Result;
      }
      Console.WriteLine(RawContent);
    }

    protected virtual IHealthNetConfiguration GetConfiguration()
    {
      return null;
    }

    protected virtual void ConfigureDependencies(IServiceCollection services)
    {

    }

    protected virtual string Path => $"/api/healthcheck{(IsIntrusive ? "?intrusive=true" : string.Empty)}";

    protected virtual bool IsIntrusive => false;

    protected HttpResponseMessage Response { get; private set; }

    protected string RawContent { get; private set; }

    protected virtual IEnumerable<ISystemChecker> CreateCheckers()
    {
      var systemChecker = Substitute.For<ISystemChecker>();
      systemChecker.CheckSystem().Returns(new SystemCheckResult());
      yield return systemChecker;
    }
  }
}