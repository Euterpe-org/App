using Euterpe.Core.JsonContexts;
using Refit;
using TUnit.Mocks.Http;

namespace Euterpe.Tests.TestSupport;

internal static class MockHttpHandlerExtensions
{
    public static T CreateEuterpeClient<T>(this MockHttpHandler handler, string basePath) =>
        RestService.ForGenerated<T>(
            handler.ThrowOnUnmatched().CreateClient($"{EuterpeApi.BaseUrl}{basePath}"),
            SnakeCaseJsonContext.Default);
}
