using System.Windows;

namespace HexEdit
{
	public class SettingsData
	{

		public string Font { get; set; } = DefaultSettings.Font;
		public int FontSize { get; set; } = DefaultSettings.FontSize;

		public double PositionLeft { get; set; }
		public double PositionTop { get; set; }
		public double Width { get; set; } = 500;
		public double Height { get; set; } = 400;
		public WindowState WindowState { get; set; }

	}
}