using System;
using System.Windows;

namespace HexEdit
{
	public class SettingsData
	{

		public string Id { get; set; } = Guid.NewGuid().ToString();

		public string Font { get; set; } = DefaultSettings.Font;
		public int FontSize { get; set; } = DefaultSettings.FontSize;

		public double PositionLeft { get; set; }
		public double PositionTop { get; set; }
		public double Width { get; set; } = 600;
		public double Height { get; set; } = 300;
		public WindowState WindowState { get; set; }

	}
}