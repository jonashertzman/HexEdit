using System.Windows;

namespace HexEdit;

internal class Utils
{

	public static void HideMinimizeAndMaximizeButtons(Window window)
	{
		window.SourceInitialized += (sender, eventArgs) =>
		{
			IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
			int style = WinApi.GetWindowLong(hwnd, WinApi.GWL_STYLE);

			_ = WinApi.SetWindowLong(hwnd, WinApi.GWL_STYLE, style & ~WinApi.WS_MAXIMIZEBOX & ~WinApi.WS_MINIMIZEBOX);
		};
	}

}
