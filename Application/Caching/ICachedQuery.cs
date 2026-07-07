using MediatR;

namespace Application.Caching;

public interface ICachedQuery<TResponse> : IRequest<TResponse>
{
    string CacheKey { get; }

    TimeSpan CacheDuration { get; }
}
