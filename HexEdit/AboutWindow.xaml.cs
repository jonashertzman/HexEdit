﻿using System.Windows;

namespace HexEdit;

public partial class AboutWindow : Window
{

	public AboutWindow()
	{
		InitializeComponent();
	}

	private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
	{
		ProcessStartInfo psi = new(e.Uri.AbsoluteUri) { UseShellExecute = true };
		Process.Start(psi);

		e.Handled = true;
	}

	private void Feedback_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
	{
		MainWindowViewModel viewModel = DataContext as MainWindowViewModel;

		string address = "jonashertzmansoftware@gmail.com";
		string subject = viewModel.FullApplicationName;
		string body = "Hello";

		string mailto = $"mailto:{address}?Subject={subject}&Body={body}";

		Process.Start(new ProcessStartInfo(mailto) { UseShellExecute = true });

		e.Handled = true;
	}

}
