public interface ICachableRequest
{
    string CacheKey { get; } // Önbellek anahtarı.
    bool BypassCache { get; } // Önbelleği atla.
    string? CacheGroupKey { get; } // Önbellek grubu.
    TimeSpan? SlidingExpiration { get; } // Süreli önbellek.
}

