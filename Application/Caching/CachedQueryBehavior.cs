using Application.Abstractions.Services;
using MediatR;

namespace Application.Caching;

public sealed class CachedQueryBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cacheService;

    public CachedQueryBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICachedQuery<TResponse> cachedQuery)
        {
            return await next();
        }

        var cachedResponse = await _cacheService.GetAsync<TResponse>(
            cachedQuery.CacheKey,
            cancellationToken);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        var response = await next();
        if (response is not null)
        {
            await _cacheService.SetAsync(
                cachedQuery.CacheKey,
                response,
                cachedQuery.CacheDuration,
                cancellationToken);
        }

        return response;
    }
}
