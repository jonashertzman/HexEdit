using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HexEdit;

/// <summary>
/// Interaction logic for OptionsWindow.xaml
/// </summary>
public partial class OptionsWindow : Window
{

	#region Members

	Rectangle selectecRectangle;

	#endregion

	#region Constructor

	public OptionsWindow()
	{
		InitializeComponent();

		foreach (FontFamily family in Fonts.SystemFontFamilies.OrderBy(x => x.Source))
		{
			ComboBoxFont.Items.Add(family.Source);
		}
	}

	#endregion

	#region Events

	private void ButtonBrowseFont_Click(object sender, RoutedEventArgs e)
	{
		//FontDialog fd = new FontDialog();
		//fd.FontMustExist = true;

		//if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		//{
		//	ComboBoxFont.Text = fd.Font.Name;
		//	TextBoxFontSize.Text = ((int)(fd.Font.Size * 96.0 / 72.0)).ToString(CultureInfo.InvariantCulture);
		//}
	}

	private void Rectangle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		selectecRectangle = e.Source as Rectangle;

		LabelA.Visibility = selectecRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;
		SliderA.Visibility = selectecRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;

		Color currentColor = Color.FromArgb((byte)(selectecRectangle == SelectionBackground ? ((SolidColorBrush)selectecRectangle.Fill).Color.A : 255), ((SolidColorBrush)selectecRectangle.Fill).Color.R, ((SolidColorBrush)selectecRectangle.Fill).Color.G, ((SolidColorBrush)selectecRectangle.Fill).Color.B);

		SliderR.Value = currentColor.R;
		SliderG.Value = currentColor.G;
		SliderB.Value = currentColor.B;
		SliderA.Value = currentColor.A;

		ColorHex.Text = currentColor.ToString();

		ColorChooser.IsOpen = true;
	}

	private void ButtonResetColors_Click(object sender, RoutedEventArgs e)
	{
		FullMatchForeground.Fill = new SolidColorBrush(DefaultSettings.TextForeground);
		FullMatchBackground.Fill = new SolidColorBrush(DefaultSettings.TextBackground);

		SelectionBackground.Fill = new SolidColorBrush(DefaultSettings.SelectionBackground);
	}

	private void ButtonResetFont_Click(object sender, RoutedEventArgs e)
	{
		ComboBoxFont.Text = DefaultSettings.Font;
		TextBoxFontSize.Text = DefaultSettings.FontSize.ToString();
	}

	private void ButtonOk_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}

	private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		byte alpha = (byte)(selectecRectangle == SelectionBackground ? (byte)SliderA.Value : 255);

		Color newColor = Color.FromArgb(alpha, (byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
		ColorHex.Text = newColor.ToString();
		selectecRectangle.Fill = new SolidColorBrush(newColor);

		SliderR.Background = new LinearGradientBrush(Color.FromArgb(alpha, 0, newColor.G, newColor.B), Color.FromArgb(alpha, 255, newColor.G, newColor.B), 0);
		SliderG.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, 0, newColor.B), Color.FromArgb(alpha, newColor.R, 255, newColor.B), 0);
		SliderB.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, newColor.G, 0), Color.FromArgb(alpha, newColor.R, newColor.G, 255), 0);
		SliderA.Background = new LinearGradientBrush(Color.FromArgb(0, newColor.R, newColor.G, newColor.B), Color.FromArgb(255, newColor.R, newColor.G, newColor.B), 0);
	}

	private void Window_SourceInitialized(object sender, EventArgs e)
	{
		IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
		int style = GetWindowLong(hwnd, GWL_STYLE);

		SetWindowLong(hwnd, GWL_STYLE, style & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
	}

	#endregion

	#region API Imports

	public const int GWL_STYLE = -16;
	public const int WS_MAXIMIZEBOX = 0x10000;
	public const int WS_MINIMIZEBOX = 0x20000;

	[DllImport("user32.dll")]
	extern internal static int GetWindowLong(IntPtr hwnd, int index);

	[DllImport("user32.dll")]
	extern internal static int SetWindowLong(IntPtr hwnd, int index, int value);

	#endregion

}
