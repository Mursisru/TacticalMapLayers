namespace TacticalMapLayers
{
    /// <summary>UI language for the tactical layers panel.</summary>
    internal enum TacticalUiLanguage
    {
        Russian = 0,
        English = 1,
    }

    /// <summary>Localized UI strings (Russian / English).</summary>
    internal static class TacticalStrings
    {
        public static TacticalUiLanguage Language { get; set; } = TacticalUiLanguage.Russian;

        private static bool En => Language == TacticalUiLanguage.English;

        public static TacticalUiLanguage Parse(string code)
        {
            if (string.IsNullOrEmpty(code))
                return TacticalUiLanguage.Russian;
            return code.Trim().StartsWith("e", System.StringComparison.OrdinalIgnoreCase)
                ? TacticalUiLanguage.English
                : TacticalUiLanguage.Russian;
        }

        public static string Code(TacticalUiLanguage lang) => lang == TacticalUiLanguage.English ? "en" : "ru";

        public static string WindowTitle => En ? "Tactical layers" : "Слои тактики";

        public static string TabTactical => En ? "Tactical" : "Тактика";
        public static string TabFront => En ? "Front" : "Фронт";
        public static string TabSettings => En ? "Settings" : "Настройки";

        public static string SectionRadars => En ? "Radars & facilities" : "Радары и объекты";
        public static string SectionAnalysis => En ? "Analysis" : "Анализ";
        public static string SectionInterface => En ? "Interface" : "Интерфейс";

        public static string LayerEnemyGround =>
            En ? "Enemy radars (land & fleet)" : "Враж. радары (суша и флот)";

        public static string LayerAllyStationary =>
            En ? "Allied stationary radars" : "Союз. стац. радары";

        public static string LayerAllyAll => En ? "All allied radars" : "Все союз. радары";

        public static string LayerEnemyAirbases =>
            En ? "Enemy bases / airfields" : "Враж. базы / аэродромы";

        public static string LayerFrontLine => En ? "Front line" : "Линия фронта";

        public static string FrontHint =>
            En
                ? "Curve between visible friendly and enemy ground/naval forces; updates with the situation."
                : "Кривая между видимыми сухопутными и морскими силами; обновляется по обстановке.";

        public static string LabelLanguage => En ? "Panel language" : "Язык панели";
        public static string LabelLangEn => "English";
        public static string LabelLangRu => "Русский";

        public static string LabelShowHints => En ? "Show front-line hint" : "Подсказка на вкладке «Фронт»";

        public static string LabelResetLayers =>
            En ? "Turn off all map overlays" : "Выключить все слои на карте";

        public static string SectionAbout => En ? "About" : "О модуле";
    }
}
