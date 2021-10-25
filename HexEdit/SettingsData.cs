using System.Windows;
using System.Windows.Media;

namespace HexEdit;

public class SettingsData
{

	public string Id { get; set; } = Guid.NewGuid().ToString();

	public string Font { get; set; } = DefaultSettings.Font;
	public int FontSize { get; set; } = DefaultSettings.FontSize;

	public int BytesPerRow { get; set; } = 8;

	public double PositionLeft { get; set; }
	public double PositionTop { get; set; }
	public double Width { get; set; } = 600;
	public double Height { get; set; } = 300;
	public WindowState WindowState { get; set; }

	public Color TextForeground { get; set; } = DefaultSettings.TextForeground;
	public Color TextBackground { get; set; } = DefaultSettings.TextBackground;

	public Color SelectionBackground { get; set; } = DefaultSettings.SelectionBackground;

}
