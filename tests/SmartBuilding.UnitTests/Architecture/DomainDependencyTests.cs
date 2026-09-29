using SmartBuilding.Domain.Entities;

namespace SmartBuilding.UnitTests.Architecture;

public class DomainDependencyTests
{
    [Fact]
    public void DomainAssembly_DoesNotReferenceInfrastructureFrameworks()
    {
        string[] forbiddenDependencies =
        [
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Npgsql",
            "SmartBuilding.Api",
            "SmartBuilding.Infrastructure"
        ];

        string[] referencedAssemblies = typeof(Building).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(
            referencedAssemblies,
            reference => forbiddenDependencies.Any(reference.StartsWith));
    }
}