using System.Collections.Concurrent;
using KairusBot.Models;

namespace KairusBot.Services;

public sealed class InMemoryUserStateService
{
    private readonly ConcurrentDictionary<long, UserSearchSession> _sessions = new();

    public UserSearchSession GetOrCreate(long userId) =>
        _sessions.GetOrAdd(userId, _ => new UserSearchSession());

    public void Reset(long userId) =>
        _sessions.TryRemove(userId, out _);

    public bool TryGet(long userId, out UserSearchSession session) =>
        _sessions.TryGetValue(userId, out session!);
}

