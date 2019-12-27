using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Media;

namespace HexEdit
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{

		#region Properties

		public string Title
		{
			get { return "Hex Edit"; }
		}

		public string Version
		{
			get { return "Alpha 1"; }
		}

		public string BuildNumber
		{
			get
			{
				DateTime buildDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
				return $"{buildDate.ToString("yy")}{buildDate.DayOfYear}";
			}
		}

		public string FullApplicationName
		{
			get { return $"{Title} {Version}  (Build {BuildNumber})"; }
		}

		public FontFamily Font
		{
			get { return AppSettings.Font; }
			set { AppSettings.Font = value; OnPropertyChanged(nameof(Font)); }
		}

		public int FontSize
		{
			get { return AppSettings.FontSize; }
			set { AppSettings.FontSize = value; OnPropertyChanged(nameof(FontSize)); }
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}
