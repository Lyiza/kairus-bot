namespace KairusBot.Utilities;

public static class VkSearchDictionaries
{
    public static readonly string[] Regions =
    [
        "Алтай",
        "Кавказ",
        "Подмосковье",
        "Крым",
        "Северо-Запад"
    ];

    public static readonly string[] Moods =
    [
        "Водоём",
        "Лес",
        "Серпантин",
        "Заброшки"
    ];

    public static readonly string[] RoadTypes =
    [
        "Асфальт",
        "Грунт",
        "Смешанный"
    ];

    public static readonly Dictionary<string, string[]> CitiesByRegion = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Алтай"] = ["Бийск", "Горно-Алтайск", "Барнаул"],
        ["Кавказ"] = ["Минеральные Воды", "Пятигорск", "Дербент"],
        ["Подмосковье"] = ["Москва", "Дмитров", "Коломна"],
        ["Крым"] = ["Симферополь", "Севастополь", "Ялта"],
        ["Северо-Запад"] = ["Санкт-Петербург", "Выборг", "Великий Новгород"]
    };
}

