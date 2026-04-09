using KairusBot.Dtos;
using KairusBot.Models;
using KairusBot.Options;
using KairusBot.Services;
using KairusBot.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace KairusBot.Controllers;

[ApiController]
[Route("vk")]
public class VkCallbackController : ControllerBase
{
    private readonly VkOptions _vkOptions;
    private readonly VkApiService _vkApi;
    private readonly InMemoryUserStateService _userState;
    private readonly RouteCatalogService _routes;
    private readonly InMemoryFavoritesService _favorites;
    private readonly ILogger<VkCallbackController> _logger;

    private const string Greeting = "Привет! Я бот Кайрус. Помогу подобрать маршрут для покатушки.";
    private const string ChooseAction = "Выбери действие";
    private const string WhatNext = "Что дальше?";
    private const string StartOver = "Начни поиск заново";
    private const string NoSavedRoutes = "У тебя пока нет сохранённых маршрутов";
    private const string TypeRouteNameHint = "Напиши название маршрута, чтобы открыть его";

    public VkCallbackController(
        IOptions<VkOptions> vkOptions,
        VkApiService vkApi,
        InMemoryUserStateService userState,
        RouteCatalogService routes,
        InMemoryFavoritesService favorites,
        ILogger<VkCallbackController> logger)
    {
        _vkOptions = vkOptions.Value;
        _vkApi = vkApi;
        _userState = userState;
        _routes = routes;
        _favorites = favorites;
        _logger = logger;
    }

    [HttpPost("callback")]
    [Consumes("application/json")]
    public async Task<IActionResult> Callback([FromBody] VkCallbackRequest? request, CancellationToken cancellationToken)
    {
        if (request?.Type is null)
        {
            return BadRequest();
        }

        switch (request.Type)
        {
            case "confirmation":
                return Content(_vkOptions.ConfirmationCode, "text/plain; charset=utf-8");

            case "message_new":
                if (!TryValidateSecret(request, out var secretError))
                {
                    return secretError;
                }

                LogIncomingMessage(request);

                var msg = request.Object?.Message;
                if (msg is not null)
                {
                    await HandleMessageNewAsync(msg.PeerId, msg.FromId, msg.Text, cancellationToken).ConfigureAwait(false);
                }

                return Content("ok", "text/plain; charset=utf-8");

            case "message_event":
                if (!TryValidateSecret(request, out var secretError2))
                {
                    return secretError2;
                }

                await HandleMessageEventAsync(request, cancellationToken).ConfigureAwait(false);
                return Content("ok", "text/plain; charset=utf-8");

            default:
                _logger.LogInformation("VK callback: type={Type}, group_id={GroupId}", request.Type, request.GroupId);
                return Content("ok", "text/plain; charset=utf-8");
        }
    }

    private void LogIncomingMessage(VkCallbackRequest request)
    {
        var m = request.Object?.Message;
        if (m is null)
        {
            _logger.LogWarning("message_new without object.message, group_id={GroupId}", request.GroupId);
            return;
        }

        _logger.LogInformation(
            "VK message_new: type={Type} group_id={GroupId} from_id={FromId} text={Text} payload={Payload}",
            request.Type,
            request.GroupId,
            m.FromId,
            m.Text ?? string.Empty,
            m.Payload ?? string.Empty);
    }

    private async Task HandleMessageNewAsync(long peerId, long userId, string? textRaw, CancellationToken cancellationToken)
    {
        var text = (textRaw ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(text) || text.Equals("Начать", StringComparison.OrdinalIgnoreCase))
        {
            _userState.Reset(userId);
            await _vkApi.SendMessageAsync(peerId, Greeting, VkKeyboardBuilder.StartKeyboardOneTime(), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (text.Equals("Помощь", StringComparison.OrdinalIgnoreCase))
        {
            await _vkApi.SendMessageAsync(
                    peerId,
                    "Чтобы найти маршрут, нажми \"Найти маршрут\" и отвечай на вопросы. В конце бот пришлёт координаты старта — откроешь в любом навигаторе и поедешь.",
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            await _vkApi.SendMessageAsync(peerId, ChooseAction, VkKeyboardBuilder.StartKeyboardOneTime(), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (text.Equals("Избранное", StringComparison.OrdinalIgnoreCase))
        {
            var routes = _favorites.GetAll(userId);
            if (routes.Count == 0)
            {
                await _vkApi.SendMessageAsync(peerId, NoSavedRoutes, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                var lines = new List<string> { "Твои маршруты:" };
                for (var i = 0; i < routes.Count; i++)
                {
                    lines.Add($"{i + 1}. {routes[i].Name}");
                }
                lines.Add(TypeRouteNameHint);

                await _vkApi.SendMessageAsync(peerId, string.Join('\n', lines), cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            await _vkApi.SendMessageAsync(peerId, ChooseAction, VkKeyboardBuilder.StartKeyboardOneTime(), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (text.Equals("В главное меню", StringComparison.OrdinalIgnoreCase))
        {
            _userState.Reset(userId);
            await _vkApi.SendMessageAsync(peerId, Greeting, VkKeyboardBuilder.StartKeyboardOneTime(), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (text.Equals("Найти маршрут", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("Найти новый маршрут", StringComparison.OrdinalIgnoreCase))
        {
            StartSearch(userId);
            await _vkApi.SendMessageAsync(peerId, "Выбери регион", VkKeyboardBuilder.RegionKeyboardOneTime(), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        // Открыть маршрут из избранного по названию (не мешаем пошаговому поиску)
        _userState.TryGet(userId, out var existingSession);
        if (existingSession is null || existingSession.CurrentStep == Models.SearchStep.None)
        {
            var opened = await TryOpenFavoriteRouteAsync(peerId, userId, text, cancellationToken).ConfigureAwait(false);
            if (opened)
            {
                return;
            }
        }

        if (existingSession is not null && existingSession.CurrentStep != Models.SearchStep.None)
        {
            var handled = await TryHandleSearchStepAsync(peerId, userId, text, existingSession, cancellationToken).ConfigureAwait(false);
            if (handled)
            {
                return;
            }
        }

        await _vkApi.SendMessageAsync(peerId, Greeting, VkKeyboardBuilder.StartKeyboardOneTime(), cancellationToken)
            .ConfigureAwait(false);
    }

    private void StartSearch(long userId)
    {
        _userState.Reset(userId);
        var session = _userState.GetOrCreate(userId);
        session.CurrentStep = Models.SearchStep.WaitingForRegion;
    }

    private async Task<bool> TryHandleSearchStepAsync(
        long peerId,
        long userId,
        string text,
        Models.UserSearchSession session,
        CancellationToken cancellationToken)
    {
        switch (session.CurrentStep)
        {
            case Models.SearchStep.WaitingForRegion:
            {
                if (!TryNormalizeRegion(text, out var region))
                {
                    await _vkApi.SendMessageAsync(peerId, "Выбери регион", VkKeyboardBuilder.RegionKeyboardOneTime(), cancellationToken)
                        .ConfigureAwait(false);
                    return true;
                }

                session.SelectedRegion = region;
                session.CurrentStep = Models.SearchStep.WaitingForCity;

                await _vkApi.SendMessageAsync(peerId, "Выбери город старта", VkKeyboardBuilder.CityKeyboardOneTime(region), cancellationToken)
                    .ConfigureAwait(false);
                return true;
            }

            case Models.SearchStep.WaitingForCity:
            {
                if (string.IsNullOrWhiteSpace(session.SelectedRegion) ||
                    !TryNormalizeCity(session.SelectedRegion, text, out var city))
                {
                    var region = session.SelectedRegion ?? string.Empty;
                    await _vkApi.SendMessageAsync(peerId, "Выбери город старта", VkKeyboardBuilder.CityKeyboardOneTime(region), cancellationToken)
                        .ConfigureAwait(false);
                    return true;
                }

                session.SelectedCity = city;
                session.CurrentStep = Models.SearchStep.WaitingForMood;

                await _vkApi.SendMessageAsync(peerId, "Выбери настроение", VkKeyboardBuilder.MoodKeyboardOneTime(), cancellationToken)
                    .ConfigureAwait(false);
                return true;
            }

            case Models.SearchStep.WaitingForMood:
            {
                if (!TryNormalizeFromList(text, VkSearchDictionaries.Moods, out var mood))
                {
                    await _vkApi.SendMessageAsync(peerId, "Выбери настроение", VkKeyboardBuilder.MoodKeyboardOneTime(), cancellationToken)
                        .ConfigureAwait(false);
                    return true;
                }

                session.SelectedMood = mood;
                session.CurrentStep = Models.SearchStep.WaitingForRoadType;

                await _vkApi.SendMessageAsync(peerId, "Выбери тип дороги", VkKeyboardBuilder.RoadTypeKeyboardOneTime(), cancellationToken)
                    .ConfigureAwait(false);
                return true;
            }

            case Models.SearchStep.WaitingForRoadType:
            {
                if (!TryNormalizeFromList(text, VkSearchDictionaries.RoadTypes, out var roadType))
                {
                    await _vkApi.SendMessageAsync(peerId, "Выбери тип дороги", VkKeyboardBuilder.RoadTypeKeyboardOneTime(), cancellationToken)
                        .ConfigureAwait(false);
                    return true;
                }

                session.SelectedRoadType = roadType;

                var region = session.SelectedRegion ?? string.Empty;
                var city = session.SelectedCity ?? string.Empty;
                var mood = session.SelectedMood ?? string.Empty;

                var route = _routes.Find(region, city, mood, roadType);
                if (route is not null)
                {
                    await _vkApi.SendMessageAsync(peerId, RouteCardFormatter.Format(route), cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    await _vkApi.SendMessageAsync(
                            peerId,
                            "Спасибо, что попробовал наш бот. Подпишись на наши другие каналы: Одноклассники и Дзен.",
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    await _vkApi.SendMessageAsync(
                            peerId,
                            "Действия:",
                            VkKeyboardBuilder.ResultInlineKeyboard(route.Name, route.StartCoordinates),
                            cancellationToken)
                        .ConfigureAwait(false);

                    await _vkApi.SendMessageAsync(peerId, WhatNext, VkKeyboardBuilder.AfterResultKeyboardOneTime(), cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await _vkApi.SendMessageAsync(peerId, "По таким фильтрам пока ничего не нашлось. Попробуй изменить выбор", cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    await _vkApi.SendMessageAsync(peerId, StartOver, VkKeyboardBuilder.AfterResultKeyboardOneTime(), cancellationToken)
                        .ConfigureAwait(false);
                }

                _userState.Reset(userId);
                return true;
            }

            default:
                return false;
        }
    }

    private static bool TryNormalizeRegion(string input, out string region) =>
        TryNormalizeFromList(input, VkSearchDictionaries.Regions, out region);

    private static bool TryNormalizeCity(string region, string input, out string city)
    {
        if (!VkSearchDictionaries.CitiesByRegion.TryGetValue(region, out var cities))
        {
            city = string.Empty;
            return false;
        }

        return TryNormalizeFromList(input, cities, out city);
    }

    private static bool TryNormalizeFromList(string input, string[] allowed, out string normalized)
    {
        foreach (var v in allowed)
        {
            if (string.Equals(v, input, StringComparison.OrdinalIgnoreCase))
            {
                normalized = v;
                return true;
            }
        }

        normalized = string.Empty;
        return false;
    }

    private async Task HandleMessageEventAsync(VkCallbackRequest request, CancellationToken cancellationToken)
    {
        var obj = request.Object;
        if (obj?.Payload is null)
        {
            _logger.LogWarning("message_event without payload, group_id={GroupId}", request.GroupId);
            return;
        }

        if (!TryParseResultInlinePayload(obj.Payload.Value, out var payload))
        {
            _logger.LogWarning("message_event payload parse failed, group_id={GroupId}", request.GroupId);

            await _vkApi.SendMessageEventAnswerAsync(
                obj.EventId ?? string.Empty,
                obj.UserId,
                obj.PeerId,
                cancellationToken).ConfigureAwait(false);

            await _vkApi.SendMessageAsync(
                obj.PeerId,
                "Не удалось обработать действие. Попробуй ещё раз.",
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return;
        }       

        _logger.LogInformation(
            "VK message_event: event_id={EventId} action={Action} peer_id={PeerId}",
            obj.EventId ?? string.Empty,
            payload.Action,
            obj.PeerId);

        if (payload.Action.Equals("build_route", StringComparison.OrdinalIgnoreCase))
        {
            await _vkApi.SendMessageEventAnswerAsync(obj.EventId ?? string.Empty, obj.UserId, obj.PeerId, cancellationToken)
                .ConfigureAwait(false);
            await _vkApi.SendMessageAsync(
                    obj.PeerId,
                    BuildStartMessage(payload.StartCoordinates),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            await _vkApi.SendMessageAsync(obj.PeerId, WhatNext, VkKeyboardBuilder.AfterResultKeyboardOneTime(), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (payload.Action.Equals("save_route", StringComparison.OrdinalIgnoreCase))
        {
            await _vkApi.SendMessageEventAnswerAsync(obj.EventId ?? string.Empty, obj.UserId, obj.PeerId, cancellationToken)
                .ConfigureAwait(false);

            var route = _routes.GetByName(payload.RouteName);
            if (route is null)
            {
                await _vkApi.SendMessageAsync(obj.PeerId, "Маршрут не найден", cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                await _vkApi.SendMessageAsync(obj.PeerId, WhatNext, VkKeyboardBuilder.AfterResultKeyboardOneTime(), cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            if (_favorites.Exists(obj.UserId, route.Name))
            {
                await _vkApi.SendMessageAsync(obj.PeerId, "Этот маршрут уже в избранном", cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                await _vkApi.SendMessageAsync(obj.PeerId, WhatNext, VkKeyboardBuilder.AfterResultKeyboardOneTime(), cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            _favorites.Add(obj.UserId, route);
            await _vkApi.SendMessageAsync(obj.PeerId, "Маршрут добавлен в избранное", cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            await _vkApi.SendMessageAsync(obj.PeerId, WhatNext, VkKeyboardBuilder.AfterResultKeyboardOneTime(), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (payload.Action.Equals("remove_favorite", StringComparison.OrdinalIgnoreCase))
        {
            await _vkApi.SendMessageEventAnswerAsync(obj.EventId ?? string.Empty, obj.UserId, obj.PeerId, cancellationToken)
                .ConfigureAwait(false);

            var removed = _favorites.Remove(obj.UserId, payload.RouteName);
            if (removed)
            {
                await _vkApi.SendMessageAsync(obj.PeerId, "Маршрут удалён из избранного", cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await _vkApi.SendMessageAsync(obj.PeerId, "Маршрут не найден в избранном", cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            await _vkApi.SendMessageAsync(obj.PeerId, WhatNext, VkKeyboardBuilder.AfterResultKeyboardOneTime(), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static string BuildStartMessage(string startCoordinates)
    {
        if (!TryParseLatLon(startCoordinates, out var lat, out var lon))
        {
            // Если вдруг не получилось собрать геоссылку — хотя бы не отдаём “цифры” как единственный вариант.
            return $"Старт: {startCoordinates}";
        }

        // Yandex expects "ll=lon,lat"
        var ll = $"{lon}%2C{lat}";
        var pt = $"{lon},{lat}";
        var link = $"https://yandex.ru/maps/?ll={ll}&z=14&pt={pt},pm2rdm";
        return $"Старт (геоточка): {lat}, {lon}\nОткрыть в навигаторе: {link}";
    }

    private static bool TryParseLatLon(string value, out string lat, out string lon)
    {
        lat = string.Empty;
        lon = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        lat = parts[0];
        lon = parts[1];
        return true;
    }

    private static bool TryParseResultInlinePayload(JsonElement element, out ResultInlinePayload payload)
    {
        try
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var s = element.GetString();
                if (string.IsNullOrWhiteSpace(s))
                {
                    payload = new ResultInlinePayload();
                    return false;
                }

                payload = JsonSerializer.Deserialize<ResultInlinePayload>(s) ?? new ResultInlinePayload();
                return !string.IsNullOrWhiteSpace(payload.Action);
            }

            payload = element.Deserialize<ResultInlinePayload>() ?? new ResultInlinePayload();
            return !string.IsNullOrWhiteSpace(payload.Action);
        }
        catch
        {
            payload = new ResultInlinePayload();
            return false;
        }
    }

    private bool TryValidateSecret(VkCallbackRequest request, out IActionResult error)
    {
        if (string.IsNullOrWhiteSpace(_vkOptions.SecretKey))
        {
            _logger.LogError("Vk SecretKey is not configured");
            error = StatusCode(StatusCodes.Status500InternalServerError);
            return false;
        }

        if (!string.Equals(request.Secret, _vkOptions.SecretKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid VK secret for group_id={GroupId}", request.GroupId);
            error = StatusCode(StatusCodes.Status403Forbidden);
            return false;
        }

        error = Content("ok", "text/plain; charset=utf-8");
        return true;
    }

    private async Task<bool> TryOpenFavoriteRouteAsync(
        long peerId,
        long userId,
        string routeName,
        CancellationToken cancellationToken)
    {
        var favorites = _favorites.GetAll(userId);
        var match = favorites.FirstOrDefault(r => string.Equals(r.Name, routeName, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return false;
        }

        await _vkApi.SendMessageAsync(peerId, RouteCardFormatter.Format(match), cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await _vkApi.SendMessageAsync(
                peerId,
                "Спасибо, что попробовал наш бот. Подпишись на наши другие каналы: Одноклассники и Дзен.",
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await _vkApi.SendMessageAsync(
                peerId,
                "Действия:",
                VkKeyboardBuilder.FavoriteRouteInlineKeyboard(match.Name, match.StartCoordinates),
                cancellationToken)
            .ConfigureAwait(false);
        await _vkApi.SendMessageAsync(peerId, WhatNext, VkKeyboardBuilder.AfterResultKeyboardOneTime(), cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}