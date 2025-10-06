using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HexEdit;

public partial class OptionsWindow : Window
{

	#region Members

	Rectangle selectedRectangle;

	#endregion

	#region Constructor

	public OptionsWindow()
	{
		InitializeComponent();

		Utils.HideMinimizeAndMaximizeButtons(this);

		foreach (FontFamily family in Fonts.SystemFontFamilies.OrderBy(x => x.Source))
		{
			ComboBoxFont.Items.Add(family.Source);
		}
	}

	#endregion

	#region Proprerties

	MainWindowViewModel ViewModel
	{
		get { return DataContext as MainWindowViewModel; }
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
		selectedRectangle = e.Source as Rectangle;

		LabelA.Visibility = selectedRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;
		SliderA.Visibility = selectedRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;

		Color currentColor = Color.FromArgb((byte)(selectedRectangle == SelectionBackground ? ((SolidColorBrush)selectedRectangle.Fill).Color.A : 255), ((SolidColorBrush)selectedRectangle.Fill).Color.R, ((SolidColorBrush)selectedRectangle.Fill).Color.G, ((SolidColorBrush)selectedRectangle.Fill).Color.B);

		SliderR.Value = currentColor.R;
		SliderG.Value = currentColor.G;
		SliderB.Value = currentColor.B;
		SliderA.Value = currentColor.A;

		ColorHex.Text = currentColor.ToString();

		ColorChooser.IsOpen = true;
	}

	private void ButtonResetColors_Click(object sender, RoutedEventArgs e)
	{
		ColorTheme themeDefaults = AppSettings.Theme switch
		{
			Themes.Light => DefaultSettings.LightTheme,
			Themes.Dark => DefaultSettings.DarkTheme,
			_ => throw new NotImplementedException(),
		};

		FullMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.TextForeground));
		FullMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.TextBackground));

		SelectionBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.SelectionBackground));

		ViewModel.UpdateTrigger++;
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
		byte alpha = (byte)(selectedRectangle == SelectionBackground ? (byte)SliderA.Value : 255);

		Color newColor = Color.FromArgb(alpha, (byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
		ColorHex.Text = newColor.ToString();
		selectedRectangle.Fill = new SolidColorBrush(newColor);

		SliderR.Background = new LinearGradientBrush(Color.FromArgb(alpha, 0, newColor.G, newColor.B), Color.FromArgb(alpha, 255, newColor.G, newColor.B), 0);
		SliderG.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, 0, newColor.B), Color.FromArgb(alpha, newColor.R, 255, newColor.B), 0);
		SliderB.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, newColor.G, 0), Color.FromArgb(alpha, newColor.R, newColor.G, 255), 0);
		SliderA.Background = new LinearGradientBrush(Color.FromArgb(0, newColor.R, newColor.G, newColor.B), Color.FromArgb(255, newColor.R, newColor.G, newColor.B), 0);

		ViewModel.UpdateTrigger++;
	}

	#endregion

}
