using System.Net;
using FluentAssertions;
using HealthNet.Integrations.Runners;
using NSubstitute;
using NUnit.Framework;

namespace HealthNet.Integrations
{
    class When_getting_the_health_status_using_a_custom_url<TFixtureRunner> : IntegrationFixtures<TFixtureRunner> where TFixtureRunner : IFixtureRunner, new()
    {
        protected override string Path
        {
            get { return "/foo/bar"; }
        }

        protected override IHealthNetConfiguration GetConfiguration()
        {
            var healthNetConfiguration = Substitute.For<IHealthNetConfiguration>();
            healthNetConfiguration.Path.Returns(Path);
            return healthNetConfiguration;
        }

        [Test]
        public void Should_return_status_of_OK()
        {
            Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}