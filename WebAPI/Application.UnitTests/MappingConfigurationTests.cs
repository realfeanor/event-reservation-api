using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.UnitTests;

public class MappingConfigurationTests
{
    [Fact]
    public void MappingConfiguration_ShouldBeValid()
    {
        MapperConfiguration configuration = new(
            expression => expression.AddMaps(typeof(ApplicationServiceRegistration).Assembly),
            NullLoggerFactory.Instance
        );

        configuration.AssertConfigurationIsValid();
    }
}
