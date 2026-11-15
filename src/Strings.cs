namespace CrosshairOverlay;

/// <summary>
/// Interface texts in the four supported languages: French, English,
/// Russian, Spanish. Every visible string in the app comes from here,
/// so adding a language is one more entry per key. French is the fallback.
/// </summary>
public static class Strings
{
    public const string French = "fr";
    public const string English = "en";
    public const string Russian = "ru";
    public const string Spanish = "es";

    public static readonly string[] All = { French, English, Russian, Spanish };

    /// <summary>Name of a language, in that language itself (shown in the picker).</summary>
    public static string LanguageName(string code)
    {
        return code switch
        {
            French => "Français",
            English => "English",
            Russian => "Русский",
            Spanish => "Español",
            _ => "Français"
        };
    }

    private static readonly Dictionary<string, Dictionary<string, string>> Table = new()
    {
        [French] = new()
        {
            ["Forme"] = "FORME",
            ["Taille"] = "TAILLE",
            ["Epaisseur"] = "ÉPAISSEUR",
            ["Couleur"] = "COULEUR",
            ["Position"] = "POSITION",
            ["Langue"] = "LANGUE",
            ["ReglagesSection"] = "RÉGLAGES",
            ["TailleMot"] = "Taille",
            ["Afficher"] = "Afficher le crosshair",
            ["ContourSombre"] = "Contour sombre",
            ["Personnalisee"] = "Personnalisée…",
            ["Actuelle"] = "Actuelle",
            ["Centrer"] = "Centrer",
            ["DeplacerSouris"] = "Déplacer à la souris",
            ["Reinitialiser"] = "Réinitialiser",
            ["Fermer"] = "Fermer",
            ["Reglages"] = "Réglages",
            ["Masquer"] = "Masquer le crosshair",
            ["Quitter"] = "Quitter",
            ["DejaEnCours"] = "Crosshair Overlay est déjà en cours d'exécution.",
            ["ErreurInattendue"] = "Erreur inattendue : ",
            ["RaccourcisIndisponibles"] = "Crosshair Overlay (raccourcis Ctrl+H / Ctrl+J indisponibles)",
            ["Blanc"] = "Blanc",
            ["Noir"] = "Noir",
            ["Rouge"] = "Rouge",
            ["Vert"] = "Vert",
            ["Jaune"] = "Jaune",
            ["Cyan"] = "Cyan",
            ["Magenta"] = "Magenta",
            ["CouleurPerso"] = "Personnalisée",
            ["Contour"] = "Contour",
            ["EpaisseurMot"] = "Épaisseur",
            ["ShapeDot"] = "Point",
            ["ShapePlus"] = "Croix +",
            ["ShapeCross"] = "Croix X",
            ["ShapeCircle"] = "Cercle",
            ["ShapeCircleDot"] = "Cercle + point",
            ["ShapeCirclePlus"] = "Cercle + croix",
            ["ShapeSquare"] = "Carré",
            ["ShapeDiamond"] = "Losange",
            ["ShapeTriangle"] = "Triangle",
            ["ShapeTee"] = "T",
            ["ShapePlusDot"] = "Croix + point",
            ["ShapePlusGap"] = "Croix + trou",
            ["ShapeLineH"] = "Ligne horizontale",
            ["ShapeLineV"] = "Ligne verticale"
        },
        [English] = new()
        {
            ["Forme"] = "SHAPE",
            ["Taille"] = "SIZE",
            ["Epaisseur"] = "THICKNESS",
            ["Couleur"] = "COLOR",
            ["Position"] = "POSITION",
            ["Langue"] = "LANGUAGE",
            ["ReglagesSection"] = "SETTINGS",
            ["TailleMot"] = "Size",
            ["Afficher"] = "Show the crosshair",
            ["ContourSombre"] = "Dark outline",
            ["Personnalisee"] = "Custom…",
            ["Actuelle"] = "Current",
            ["Centrer"] = "Center",
            ["DeplacerSouris"] = "Move with mouse",
            ["Reinitialiser"] = "Reset",
            ["Fermer"] = "Close",
            ["Reglages"] = "Settings",
            ["Masquer"] = "Hide the crosshair",
            ["Quitter"] = "Quit",
            ["DejaEnCours"] = "Crosshair Overlay is already running.",
            ["ErreurInattendue"] = "Unexpected error: ",
            ["RaccourcisIndisponibles"] = "Crosshair Overlay (Ctrl+H / Ctrl+J shortcuts unavailable)",
            ["Blanc"] = "White",
            ["Noir"] = "Black",
            ["Rouge"] = "Red",
            ["Vert"] = "Green",
            ["Jaune"] = "Yellow",
            ["Cyan"] = "Cyan",
            ["Magenta"] = "Magenta",
            ["CouleurPerso"] = "Custom",
            ["Contour"] = "Outline",
            ["EpaisseurMot"] = "Thickness",
            ["ShapeDot"] = "Dot",
            ["ShapePlus"] = "Cross +",
            ["ShapeCross"] = "Cross X",
            ["ShapeCircle"] = "Circle",
            ["ShapeCircleDot"] = "Circle + dot",
            ["ShapeCirclePlus"] = "Circle + cross",
            ["ShapeSquare"] = "Square",
            ["ShapeDiamond"] = "Diamond",
            ["ShapeTriangle"] = "Triangle",
            ["ShapeTee"] = "T",
            ["ShapePlusDot"] = "Cross + dot",
            ["ShapePlusGap"] = "Cross + gap",
            ["ShapeLineH"] = "Horizontal line",
            ["ShapeLineV"] = "Vertical line"
        },
        [Russian] = new()
        {
            ["Forme"] = "ФОРМА",
            ["Taille"] = "РАЗМЕР",
            ["Epaisseur"] = "ТОЛЩИНА",
            ["Couleur"] = "ЦВЕТ",
            ["Position"] = "ПОЗИЦИЯ",
            ["Langue"] = "ЯЗЫК",
            ["ReglagesSection"] = "НАСТРОЙКИ",
            ["TailleMot"] = "Размер",
            ["Afficher"] = "Показать прицел",
            ["ContourSombre"] = "Тёмный контур",
            ["Personnalisee"] = "Свой…",
            ["Actuelle"] = "Текущий",
            ["Centrer"] = "По центру",
            ["DeplacerSouris"] = "Перемещать мышью",
            ["Reinitialiser"] = "Сбросить",
            ["Fermer"] = "Закрыть",
            ["Reglages"] = "Настройки",
            ["Masquer"] = "Скрыть прицел",
            ["Quitter"] = "Выйти",
            ["DejaEnCours"] = "Crosshair Overlay уже запущен.",
            ["ErreurInattendue"] = "Неожиданная ошибка: ",
            ["RaccourcisIndisponibles"] = "Crosshair Overlay (сочетания Ctrl+H / Ctrl+J недоступны)",
            ["Blanc"] = "Белый",
            ["Noir"] = "Чёрный",
            ["Rouge"] = "Красный",
            ["Vert"] = "Зелёный",
            ["Jaune"] = "Жёлтый",
            ["Cyan"] = "Голубой",
            ["Magenta"] = "Пурпурный",
            ["CouleurPerso"] = "Свой",
            ["Contour"] = "Контур",
            ["EpaisseurMot"] = "Толщина",
            ["ShapeDot"] = "Точка",
            ["ShapePlus"] = "Крест +",
            ["ShapeCross"] = "Крест X",
            ["ShapeCircle"] = "Круг",
            ["ShapeCircleDot"] = "Круг + точка",
            ["ShapeCirclePlus"] = "Круг + крест",
            ["ShapeSquare"] = "Квадрат",
            ["ShapeDiamond"] = "Ромб",
            ["ShapeTriangle"] = "Треугольник",
            ["ShapeTee"] = "T",
            ["ShapePlusDot"] = "Крест + точка",
            ["ShapePlusGap"] = "Крест + разрыв",
            ["ShapeLineH"] = "Горизонтальная линия",
            ["ShapeLineV"] = "Вертикальная линия"
        },
        [Spanish] = new()
        {
            ["Forme"] = "FORMA",
            ["Taille"] = "TAMAÑO",
            ["Epaisseur"] = "GROSOR",
            ["Couleur"] = "COLOR",
            ["Position"] = "POSICIÓN",
            ["Langue"] = "IDIOMA",
            ["ReglagesSection"] = "AJUSTES",
            ["TailleMot"] = "Tamaño",
            ["Afficher"] = "Mostrar la mira",
            ["ContourSombre"] = "Contorno oscuro",
            ["Personnalisee"] = "Personalizado…",
            ["Actuelle"] = "Actual",
            ["Centrer"] = "Centrar",
            ["DeplacerSouris"] = "Mover con el ratón",
            ["Reinitialiser"] = "Restablecer",
            ["Fermer"] = "Cerrar",
            ["Reglages"] = "Ajustes",
            ["Masquer"] = "Ocultar la mira",
            ["Quitter"] = "Salir",
            ["DejaEnCours"] = "Crosshair Overlay ya se está ejecutando.",
            ["ErreurInattendue"] = "Error inesperado: ",
            ["RaccourcisIndisponibles"] = "Crosshair Overlay (atajos Ctrl+H / Ctrl+J no disponibles)",
            ["Blanc"] = "Blanco",
            ["Noir"] = "Negro",
            ["Rouge"] = "Rojo",
            ["Vert"] = "Verde",
            ["Jaune"] = "Amarillo",
            ["Cyan"] = "Cian",
            ["Magenta"] = "Magenta",
            ["CouleurPerso"] = "Personalizado",
            ["Contour"] = "Contorno",
            ["EpaisseurMot"] = "Grosor",
            ["ShapeDot"] = "Punto",
            ["ShapePlus"] = "Cruz +",
            ["ShapeCross"] = "Cruz X",
            ["ShapeCircle"] = "Círculo",
            ["ShapeCircleDot"] = "Círculo + punto",
            ["ShapeCirclePlus"] = "Círculo + cruz",
            ["ShapeSquare"] = "Cuadrado",
            ["ShapeDiamond"] = "Rombo",
            ["ShapeTriangle"] = "Triángulo",
            ["ShapeTee"] = "T",
            ["ShapePlusDot"] = "Cruz + punto",
            ["ShapePlusGap"] = "Cruz + hueco",
            ["ShapeLineH"] = "Línea horizontal",
            ["ShapeLineV"] = "Línea vertical"
        }
    };

    /// <summary>Looks up a text in the given language, falling back to French.</summary>
    public static string Get(string language, string key)
    {
        if (Table.TryGetValue(language, out var dict) && dict.TryGetValue(key, out var value))
        {
            return value;
        }
        return Table[French][key];
    }

    /// <summary>Localized name of a shape.</summary>
    public static string ShapeName(string language, CrosshairShape shape)
    {
        return Get(language, shape switch
        {
            CrosshairShape.Dot => "ShapeDot",
            CrosshairShape.Plus => "ShapePlus",
            CrosshairShape.Cross => "ShapeCross",
            CrosshairShape.Circle => "ShapeCircle",
            CrosshairShape.CircleDot => "ShapeCircleDot",
            CrosshairShape.CirclePlus => "ShapeCirclePlus",
            CrosshairShape.Square => "ShapeSquare",
            CrosshairShape.Diamond => "ShapeDiamond",
            CrosshairShape.Triangle => "ShapeTriangle",
            CrosshairShape.Tee => "ShapeTee",
            CrosshairShape.PlusDot => "ShapePlusDot",
            CrosshairShape.PlusGap => "ShapePlusGap",
            CrosshairShape.LineH => "ShapeLineH",
            CrosshairShape.LineV => "ShapeLineV",
            _ => "ShapeDot"
        });
    }

    /// <summary>Localized name of a color (by its HTML code).</summary>
    public static string ColorName(string language, string html)
    {
        return Get(language, html.ToUpperInvariant() switch
        {
            "#FFFFFF" => "Blanc",
            "#000000" => "Noir",
            "#FF0000" => "Rouge",
            "#00FF00" => "Vert",
            "#FFFF00" => "Jaune",
            "#00FFFF" => "Cyan",
            "#FF00FF" => "Magenta",
            _ => "CouleurPerso"
        });
    }
}
