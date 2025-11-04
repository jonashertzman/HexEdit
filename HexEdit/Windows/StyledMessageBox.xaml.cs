using System.Windows;

namespace HexEdit;

public partial class StyledMessageBox : Window
{
	public StyledMessageBox()
	{
		InitializeComponent();

		DataContext = this;
	}

	public string Text { get; set; }

	public string Caption { get; set; }

	public static void Show(string text, string title)
	{
		StyledMessageBox msgBox = new()
		{
			Owner = Application.Current.MainWindow,
			Text = text,
			Caption = title
		};

		msgBox.ShowDialog();
	}

}
