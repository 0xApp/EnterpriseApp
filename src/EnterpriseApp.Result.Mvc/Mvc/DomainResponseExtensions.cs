using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseApp.Result.Mvc;

public static class DomainResponseExtensions
{
    private static readonly ConcurrentDictionary<Type, Delegate> ErrorResultMap = new();

    public static IServiceCollection MapErrorResult<TError>(
        this IServiceCollection services, Func<TError, int> mapper)
    {
        ErrorResultMap[typeof(TError)] = mapper;
        return services;
    }

    public static IServiceCollection MapErrorResult<TError>(
        this IServiceCollection services, int errorCode)
    {
        ErrorResultMap[typeof(TError)] = new Func<TError, int>(_ => errorCode);
        return services;
    }

    internal static int ResolveStatusCode<TError>(TError error)
    {
        if (ErrorResultMap.TryGetValue(typeof(TError), out var mapper) && mapper is Func<TError, int> typedMapper)
        {
            return typedMapper(error);

        }
        return StatusCodes.Status422UnprocessableEntity;
    }
}
