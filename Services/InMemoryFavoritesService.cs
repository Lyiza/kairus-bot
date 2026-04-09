using System.Collections.Concurrent;
using KairusBot.Models;

namespace KairusBot.Services;

public sealed class InMemoryFavoritesService
{
    private readonly ConcurrentDictionary<long, List<RouteCard>> _favorites = new();

    public void Add(long userId, RouteCard route)
    {
        var list = _favorites.GetOrAdd(userId, _ => []);
        lock (list)
        {
            if (list.Any(r => string.Equals(r.Name, route.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            list.Add(route);
        }
    }

    public IReadOnlyList<RouteCard> GetAll(long userId)
    {
        if (!_favorites.TryGetValue(userId, out var list))
        {
            return [];
        }

        lock (list)
        {
            return list.ToList();
        }
    }

    public bool Exists(long userId, string routeName)
    {
        if (!_favorites.TryGetValue(userId, out var list))
        {
            return false;
        }

        lock (list)
        {
            return list.Any(r => string.Equals(r.Name, routeName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool Remove(long userId, string routeName)
    {
        if (!_favorites.TryGetValue(userId, out var list))
        {
            return false;
        }

        lock (list)
        {
            var idx = list.FindIndex(r => string.Equals(r.Name, routeName, StringComparison.OrdinalIgnoreCase));
            if (idx < 0)
            {
                return false;
            }

            list.RemoveAt(idx);
            return true;
        }
    }
}

