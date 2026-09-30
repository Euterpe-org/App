using Euterpe.Core.Http.Handlers;
using Euterpe.Core.JsonContexts;
using Refit;

namespace Euterpe.Core.Extensions;

public static class RefitExtensions
{
    public static IHttpClientBuilder AddEuterpeRefitClient<T>(this IServiceCollection services, string name, string basePath, bool authenticated = false)
        where T : class
    {
        var builder = services
            .AddRefitGeneratedClient<T>(RefitSettings.ForJsonContext(SnakeCaseJsonContext.Default), name)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{EuterpeApi.BaseUrl}{basePath}"))
            .AddHttpMessageHandler<XRequestIdHandler>();

        if (authenticated)
        {
            builder.AddHttpMessageHandler<AuthHeaderHandler>();
        }

        return builder;
    }
}
