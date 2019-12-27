using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;

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
