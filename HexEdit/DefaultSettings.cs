using System.Windows.Media;

namespace HexEdit;

public static class DefaultSettings
{

	internal static string Font { get; } = "Courier New";
	internal static int FontSize { get; } = 11;

	internal static Color TextForeground { get; } = Colors.Black;
	internal static Color TextBackground { get; } = Colors.White;
	internal static Color SelectionBackground { get; } = Color.FromArgb(50, 0, 150, 210);

}
