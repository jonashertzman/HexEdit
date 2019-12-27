using System;
using System.Windows;

namespace HexEdit
{
	public partial class MainWindow : Window
	{

		#region Members

		MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();

		#endregion

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();

			DataContext = ViewModel;
		}

		#endregion

		#region Events

		private void Window_Closed(object sender, EventArgs e)
		{

		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{

		}

		private void Window_Initialized(object sender, EventArgs e)
		{

		}

		#endregion

	}
}
