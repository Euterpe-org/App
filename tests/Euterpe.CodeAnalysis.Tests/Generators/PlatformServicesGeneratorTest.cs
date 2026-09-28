namespace Euterpe.CodeAnalysis.Tests.Generators;

[TestSubject(typeof(PlatformServicesGenerator))]
[Category("PlatformServicesGeneratorTests")]
public sealed class PlatformServicesGeneratorTest
{
    private const string Source = """
                                  namespace Euterpe.Abstractions
                                  {
                                      using Euterpe.Shared.Attributes;
                                      using Euterpe.Shared.DependencyInjection;

                                      [PlatformService(ServiceRegistrationLifetime.AppSingleton)]
                                      public interface IPlatformLauncher;

                                      [PlatformService]
                                      public interface IGamePathDiscovery;
                                  }

                                  namespace Euterpe.Shared.DependencyInjection
                                  {
                                      public enum ServiceRegistrationLifetime
                                      {
                                          AppSingleton = 0,
                                          PerGame = 1
                                      }
                                  }

                                  namespace Euterpe.Shared.Attributes
                                  {
                                      using Euterpe.Shared.DependencyInjection;

                                      [System.AttributeUsage(
                                          System.AttributeTargets.Interface,
                                          Inherited = false)]
                                      public sealed class PlatformServiceAttribute : System.Attribute
                                      {
                                          public PlatformServiceAttribute(
                                              ServiceRegistrationLifetime lifetime = ServiceRegistrationLifetime.PerGame)
                                          {
                                          }
                                      }
                                  }

                                  namespace Sample
                                  {
                                      using System.Runtime.Versioning;
                                      using Euterpe.Abstractions;

                                      [SupportedOSPlatform("Windows")]
                                      internal sealed class WindowsLauncher : IPlatformLauncher;

                                      [SupportedOSPlatform("Windows")]
                                      internal sealed class WindowsGamePathDiscovery : IGamePathDiscovery;
                                  }
                                  """;

    [Test]
    public void RunGenerators_PlatformServices_RegistersCurrentProjectByLifetime() =>
        Snapshot.Validate(GeneratorTestHelper.Run<PlatformServicesGenerator>(Source));
}
