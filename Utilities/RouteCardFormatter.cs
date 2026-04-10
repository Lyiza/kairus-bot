using KairusBot.Models;
using System.Globalization;

namespace KairusBot.Utilities;

public static class RouteCardFormatter
{
    public static string Format(RouteCard route, bool includeStart = true)
    {
        var title = $"**{route.Name}**";
        var vibe = BuildVibeLine(route);

        var baseText =
$@"{title}
{vibe}

Регион: {route.Region}

Город старта: {route.City}

Настроение: {route.Mood}

Тип дороги: {route.RoadType}

Описание: {route.Description}

Ключевые точки: {route.Highlights}";

        if (!includeStart)
        {
            return baseText;
        }

        var start = BuildStartLine(route.StartCoordinates);
        return $"{baseText}\n\n{start}";
    }

    private static string BuildVibeLine(RouteCard route)
    {
        var mood = string.IsNullOrWhiteSpace(route.Mood) ? "по настроению" : route.Mood.ToLowerInvariant();
        var road = string.IsNullOrWhiteSpace(route.RoadType) ? "дороги" : route.RoadType.ToLowerInvariant();
        var city = string.IsNullOrWhiteSpace(route.City) ? "города" : route.City;

        return $"Вайб: короткий выезд из {city} — {mood}, {road}, без суеты. Собери плейлист, залей полный бак и поехали.";
    }

    private static string BuildStartLine(string startCoordinates)
    {
        if (!TryParseLatLon(startCoordinates, out var lat, out var lon))
        {
            return $"Старт: {startCoordinates}";
        }

        var link = BuildYandexMapsLink(lat, lon);
        return $"Старт (геоточка): {lat.ToString("0.####", CultureInfo.InvariantCulture)}, {lon.ToString("0.####", CultureInfo.InvariantCulture)}\nОткрыть в навигаторе: {link}";
    }

    private static bool TryParseLatLon(string value, out double lat, out double lon)
    {
        lat = 0;
        lon = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        return
            double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat) &&
            double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon);
    }

    private static string BuildYandexMapsLink(double lat, double lon)
    {
        // Yandex expects "ll=lon,lat"
        var ll = $"{lon.ToString(CultureInfo.InvariantCulture)}%2C{lat.ToString(CultureInfo.InvariantCulture)}";
        var pt = $"{lon.ToString(CultureInfo.InvariantCulture)},{lat.ToString(CultureInfo.InvariantCulture)}";
        return $"https://yandex.ru/maps/?ll={ll}&z=14&pt={pt},pm2rdm";
    }
}

