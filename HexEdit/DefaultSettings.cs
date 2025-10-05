﻿namespace HexEdit;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;

	internal static ColorTheme DarkTheme { get; } = new ColorTheme()
	{
		// Editor colors
		TextBackground = "#FF1B1B1B",
		TextForeground = "#FFFFFFFF",

		SelectionBackground = "#320096D2",

		// UI colors
		NormalText = "#FFD8D8D8",
		DisabledText = "#FF888888",

		DisabledBackground = "#FF444444",

		WindowBackground = "#FF0B0B0B",
		DialogBackground = "#FF171717",

		ControlLightBackground = "#FF262626",
		ControlDarkBackground = "#FF3F3F3F",

		BorderLight = "#FF323232",
		BorderDark = "#FF595959",

		HighlightBackground = "#FF112E3C",
		HighlightBorder = "#FF2F7999",

		AttentionBackground = "#FF5C2626",
	};

	internal static ColorTheme LightTheme { get; } = new ColorTheme()
	{
		// Editor colors
		TextForeground = "#FF000000",
		TextBackground = "#FFFFFFFF",

		SelectionBackground = "#320096D2",

		// UI colors
		NormalText = "#FF000000",
		DisabledText = "#FF888888",

		DisabledBackground = "#FFAAAAAA",

		WindowBackground = "#FFFFFFFF",
		DialogBackground = "#FFEBEBEB",

		ControlLightBackground = "#FFFFFFFF",
		ControlDarkBackground = "#FFD9D9D9",

		BorderLight = "#FFCFCFCF",
		BorderDark = "#FFAAAAAA",

		HighlightBackground = "#FFDCECFC",
		HighlightBorder = "#FF7EB4EA",

		AttentionBackground = "#FFFF9F9D",
	};
}
