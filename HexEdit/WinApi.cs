using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace HexEdit;

internal class WinApi
{

	#region Constants

	public const int GWL_STYLE = -16;
	public const int WS_MAXIMIZEBOX = 0x10000;
	public const int WS_MINIMIZEBOX = 0x20000;
	private const int DWMWA_CAPTION_COLOR = 35;

	#endregion

	#region DllImports

	[DllImport("user32.dll")]
	extern public static int GetWindowLong(IntPtr hwnd, int index);

	[DllImport("user32.dll")]
	extern public static int SetWindowLong(IntPtr hwnd, int index, int value);

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

	#endregion

	#region Methods

	public static void SetTitleBarColor(Window window, bool active)
	{
		IntPtr hwnd = new WindowInteropHelper(window).Handle;
		if (hwnd == IntPtr.Zero) return;
		Color color = active ? SystemParameters.WindowGlassColor : ((SolidColorBrush)AppSettings.DialogBackground).Color;
		int colorref = color.R | (color.G << 8) | (color.B << 16); // COLORREF is 0x00BBGGRR
		DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorref, sizeof(int));
	}

	#endregion

}
