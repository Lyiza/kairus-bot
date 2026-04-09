using KairusBot.Models;

namespace KairusBot.Services;

public sealed class RouteCatalogService
{
    private readonly List<RouteCard> _routes =
    [
        // Алтай
        new RouteCard
        {
            Name = "Дорога на озеро Ая",
            Region = "Алтай",
            City = "Бийск",
            Mood = "Водоём",
            RoadType = "Асфальт",
            Description = "Красивый маршрут к озеру в предгорьях Алтая. Дорога ровная, асфальт хороший.",
            Highlights = "По пути: кафе «Старый мельник», смотровая на 45-м км, заправка Лукойл.",
            StartCoordinates = "52.5393, 85.2146"
        },
        new RouteCard
        {
            Name = "Чуйский тракт до Манжерока",
            Region = "Алтай",
            City = "Горно-Алтайск",
            Mood = "Лес",
            RoadType = "Асфальт",
            Description = "Лёгкая поездка по красивой долине Катуни. Подходит для спокойной покатушки.",
            Highlights = "Кафе у реки, вид на Катунь, фотостоп у мостов.",
            StartCoordinates = "51.9581, 85.9603"
        },
        new RouteCard
        {
            Name = "Барнаул → Змеиногорский тракт (панорамы)",
            Region = "Алтай",
            City = "Барнаул",
            Mood = "Серпантин",
            RoadType = "Смешанный",
            Description = "Повороты и перепады высот, местами участки похуже. Больше драйва, меньше спешки.",
            Highlights = "Смотровые точки, полевые дороги к панорамам, закатные виды.",
            StartCoordinates = "53.3474, 83.7788"
        },

        // Кавказ
        new RouteCard
        {
            Name = "МинВоды → Джилы-Су (виды на Эльбрус)",
            Region = "Кавказ",
            City = "Минеральные Воды",
            Mood = "Серпантин",
            RoadType = "Асфальт",
            Description = "Классический горный выезд с серпантинами и смотровыми. Лучше ехать днём.",
            Highlights = "Смотровые площадки, пастбища, панорамы Эльбруса.",
            StartCoordinates = "44.2100, 43.1350"
        },
        new RouteCard
        {
            Name = "Пятигорск → Суворовские термальные источники",
            Region = "Кавказ",
            City = "Пятигорск",
            Mood = "Водоём",
            RoadType = "Асфальт",
            Description = "Спокойный маршрут до терм. Подойдёт как короткая покатушка с отдыхом.",
            Highlights = "Кафе по пути, термальные бассейны, виды на Кавказские МинВоды.",
            StartCoordinates = "44.0394, 43.0708"
        },
        new RouteCard
        {
            Name = "Дербент → крепость Нарын-Кала и старый город",
            Region = "Кавказ",
            City = "Дербент",
            Mood = "Заброшки",
            RoadType = "Асфальт",
            Description = "Городская культурная покатушка: крепость, улочки, смотровые точки.",
            Highlights = "Нарын-Кала, старые кварталы, панорама Каспия.",
            StartCoordinates = "42.0578, 48.2891"
        },

        // Подмосковье
        new RouteCard
        {
            Name = "Москва → Дмитров (через водохранилища)",
            Region = "Подмосковье",
            City = "Москва",
            Mood = "Водоём",
            RoadType = "Асфальт",
            Description = "Лёгкий выезд из Москвы с остановками у воды и спокойными дорогами.",
            Highlights = "Береговые точки, кафе, фотостопы у воды.",
            StartCoordinates = "55.7558, 37.6173"
        },
        new RouteCard
        {
            Name = "Дмитров → карьеры и лесные дороги",
            Region = "Подмосковье",
            City = "Дмитров",
            Mood = "Лес",
            RoadType = "Грунт",
            Description = "Небольшая грунтовая покатушка по лесным дорогам. После дождя осторожнее.",
            Highlights = "Лесные просеки, карьеры, тихие места для пикника.",
            StartCoordinates = "56.3449, 37.5203"
        },
        new RouteCard
        {
            Name = "Коломна → старые усадьбы и заброшенные постройки",
            Region = "Подмосковье",
            City = "Коломна",
            Mood = "Заброшки",
            RoadType = "Смешанный",
            Description = "Маршрут по окраинам и просёлкам: старые здания, тихие дороги.",
            Highlights = "Усадьбы, старые кирпичные здания, панорамные поля.",
            StartCoordinates = "55.0948, 38.7657"
        },

        // Крым
        new RouteCard
        {
            Name = "Симферополь → Ангарский перевал",
            Region = "Крым",
            City = "Симферополь",
            Mood = "Серпантин",
            RoadType = "Асфальт",
            Description = "Короткий горный выезд с поворотами и красивыми видами.",
            Highlights = "Перевал, смотровые, сосновые участки.",
            StartCoordinates = "44.9521, 34.1024"
        },
        new RouteCard
        {
            Name = "Севастополь → мыс Фиолент",
            Region = "Крым",
            City = "Севастополь",
            Mood = "Водоём",
            RoadType = "Асфальт",
            Description = "Морская классика: виды на скалы и бухты, хороший асфальт.",
            Highlights = "Смотровые на море, бухты, точка для фото на закате.",
            StartCoordinates = "44.6167, 33.5254"
        },
        new RouteCard
        {
            Name = "Ялта → заброшенные санатории (без заезда внутрь)",
            Region = "Крым",
            City = "Ялта",
            Mood = "Заброшки",
            RoadType = "Смешанный",
            Description = "Покатушка по окрестностям с остановками у старых объектов. Безопасность прежде всего.",
            Highlights = "Архитектура, видовые точки, тихие дороги.",
            StartCoordinates = "44.4952, 34.1663"
        },

        // Северо-Запад
        new RouteCard
        {
            Name = "Петербург → Выборг (с заездом к заливу)",
            Region = "Северо-Запад",
            City = "Санкт-Петербург",
            Mood = "Водоём",
            RoadType = "Асфальт",
            Description = "Комфортный выезд к заливу и дальше в сторону Выборга. Хорошо на выходные.",
            Highlights = "Берег залива, кафе, старые улочки Выборга.",
            StartCoordinates = "59.9311, 30.3609"
        },
        new RouteCard
        {
            Name = "Выборг → лесные дороги к озёрам",
            Region = "Северо-Запад",
            City = "Выборг",
            Mood = "Лес",
            RoadType = "Грунт",
            Description = "Лесные дороги и озёра Карельского перешейка. Сухая погода — самый кайф.",
            Highlights = "Озёра, сосновые леса, места для привала.",
            StartCoordinates = "60.7097, 28.7498"
        },
        new RouteCard
        {
            Name = "Великий Новгород → старые дороги и серпантины на холмах",
            Region = "Северо-Запад",
            City = "Великий Новгород",
            Mood = "Серпантин",
            RoadType = "Смешанный",
            Description = "Неровный рельеф и повороты на холмах, местами просёлок. Ехать не спеша.",
            Highlights = "Панорамы полей, тихие деревни, смотровые на холмах.",
            StartCoordinates = "58.5215, 31.2755"
        }
    ];

    public RouteCard? Find(string region, string city, string mood, string roadType)
    {
        var matches = _routes
            .Where(r =>
                string.Equals(r.Region, region, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.City, city, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.Mood, mood, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.RoadType, roadType, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
        {
            return null;
        }

        return matches[Random.Shared.Next(matches.Count)];
    }

    public RouteCard? GetByName(string routeName)
    {
        if (string.IsNullOrWhiteSpace(routeName))
        {
            return null;
        }

        return _routes.FirstOrDefault(r => string.Equals(r.Name, routeName, StringComparison.OrdinalIgnoreCase));
    }
}

