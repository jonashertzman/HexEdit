using System;
using System.Collections.ObjectModel;
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
			get { return $"{CurrentFile ?? "New"} - {ApplicationName}"; }
		}

		public string ApplicationName
		{
			get { return "Hex Edit"; }
		}

		public string ApplicationVersion
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
			get { return $"{Title} {ApplicationVersion}  (Build {BuildNumber})"; }
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


		string currentFile;
		public string CurrentFile
		{
			get { return currentFile; }
			set { currentFile = value; OnPropertyChanged(nameof(CurrentFile)); OnPropertyChanged(nameof(Title)); }
		}

		ObservableCollection<byte> fileContent = new ObservableCollection<byte>();
		public ObservableCollection<byte> FileContent
		{
			get { return fileContent; }
			set { fileContent = value; OnPropertyChanged(nameof(FileContent)); }
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
