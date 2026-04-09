using System.Text.Json;
using KairusBot.Models;

namespace KairusBot.Utilities;

public static class VkKeyboardBuilder
{
    public static string StartKeyboardOneTime()
    {
        return Serialize(new
        {
            one_time = true,
            buttons = new object[][]
            {
                new object[]
                {
                    TextButton("Найти маршрут", "primary"),
                    TextButton("Избранное", "secondary")
                },
                new object[]
                {
                    TextButton("Помощь", "secondary")
                }
            }
        });
    }

    public static string RegionKeyboardOneTime()
    {
        var rows = VkSearchDictionaries.Regions
            .Select(r => new object[] { TextButton(r, "primary") })
            .ToArray();

        return Serialize(new { one_time = true, buttons = rows });
    }

    public static string CityKeyboardOneTime(string region)
    {
        if (!VkSearchDictionaries.CitiesByRegion.TryGetValue(region ?? string.Empty, out var cities))
        {
            cities = [];
        }

        var rows = cities
            .Select(c => new object[] { TextButton(c, "primary") })
            .ToArray();

        return Serialize(new { one_time = true, buttons = rows });
    }

    public static string MoodKeyboardOneTime()
    {
        var rows = VkSearchDictionaries.Moods
            .Select(m => new object[] { TextButton(m, "primary") })
            .ToArray();

        return Serialize(new { one_time = true, buttons = rows });
    }

    public static string RoadTypeKeyboardOneTime()
    {
        var rows = VkSearchDictionaries.RoadTypes
            .Select(t => new object[] { TextButton(t, "primary") })
            .ToArray();

        return Serialize(new { one_time = true, buttons = rows });
    }

    public static string AfterResultKeyboardOneTime()
    {
        return Serialize(new
        {
            one_time = true,
            buttons = new object[][]
            {
                new object[]
                {
                    TextButton("Найти новый маршрут", "primary"),
                    TextButton("В главное меню", "secondary")
                },
                new object[]
                {
                    TextButton("Избранное", "secondary")
                }
            }
        });
    }

    public static string ResultInlineKeyboard(string routeName, string startCoordinates)
    {
        return Serialize(new
        {
            inline = true,
            buttons = new object[][]
            {
                new object[]
                {
                    CallbackButton(
                        "Открыть старт",
                        new ResultInlinePayload
                        {
                            Action = "build_route",
                            RouteName = routeName ?? string.Empty,
                            StartCoordinates = startCoordinates ?? string.Empty
                        })
                },
                new object[]
                {
                    CallbackButton(
                        "В избранное",
                        new ResultInlinePayload
                        {
                            Action = "save_route",
                            RouteName = routeName ?? string.Empty,
                            StartCoordinates = startCoordinates ?? string.Empty
                        })
                }
            }
        });
    }

    public static string FavoriteRouteInlineKeyboard(string routeName, string startCoordinates)
    {
        return Serialize(new
        {
            inline = true,
            buttons = new object[][]
            {
                new object[]
                {
                    CallbackButton(
                        "Открыть старт",
                        new ResultInlinePayload
                        {
                            Action = "build_route",
                            RouteName = routeName ?? string.Empty,
                            StartCoordinates = startCoordinates ?? string.Empty
                        })
                },
                new object[]
                {
                    CallbackButton(
                        "Убрать из избранного",
                        new ResultInlinePayload
                        {
                            Action = "remove_favorite",
                            RouteName = routeName ?? string.Empty,
                            StartCoordinates = startCoordinates ?? string.Empty
                        })
                }
            }
        });
    }

    private static object TextButton(string label, string color) => new
    {
        action = new { type = "text", label },
        color
    };

    private static object CallbackButton(string label, ResultInlinePayload payload) => new
    {
        action = new
        {
            type = "callback",
            label,
            payload
        }
    };

    private static string Serialize(object value) =>
        JsonSerializer.Serialize(value, new JsonSerializerOptions { PropertyNamingPolicy = null });
}

