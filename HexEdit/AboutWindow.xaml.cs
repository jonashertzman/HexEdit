using System;
using System.Diagnostics;
using System.Windows;

namespace HexEdit
{
	public partial class AboutWindow : Window
	{

		public AboutWindow()
		{
			InitializeComponent();
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			ProcessStartInfo psi = new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true };
			Process.Start(psi);

			e.Handled = true;
		}

		private void Feedback_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			MainWindowViewModel viewModel = DataContext as MainWindowViewModel;

			string mailto = Uri.EscapeUriString($"mailto:jonashertzmansoftware@gmail.com?Subject={viewModel.FullApplicationName}&Body=Hello");
			ProcessStartInfo psi = new ProcessStartInfo(mailto) { UseShellExecute = true };
			Process.Start(psi);

			e.Handled = true;
		}

	}
}
