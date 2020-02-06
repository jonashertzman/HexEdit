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
				return $"{buildDate.ToString("yy")}{buildDate.DayOfYear.ToString("D3")}";
			}
		}

		public string FullApplicationName
		{
			get { return $"{ApplicationName} {ApplicationVersion}  (Build {BuildNumber})"; }
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

		PreviewMode filePreview;
		public PreviewMode FilePreview
		{
			get { return filePreview; }
			set { filePreview = value; OnPropertyChanged(nameof(FilePreview)); OnPropertyChanged(nameof(SelectedPreview)); }
		}

		public int SelectedPreview
		{
			get { return (int)FilePreview; }
			set { FilePreview = (PreviewMode)value; }
		}

		ObservableCollection<byte> fileContent = new ObservableCollection<byte>();
		public ObservableCollection<byte> FileContent
		{
			get { return fileContent; }
			set { fileContent = value; OnPropertyChanged(nameof(FileContent)); }
		}

		ObservableCollection<Chunk> chunks = new ObservableCollection<Chunk>();
		public ObservableCollection<Chunk> Chunks
		{
			get { return chunks; }
			set { chunks = value; OnPropertyChanged(nameof(Chunks)); }
		}

		int bytesPerRows = 8;
		public int BytesPerRow
		{
			get { return bytesPerRows; }
			set { bytesPerRows = value; OnPropertyChanged(nameof(BytesPerRow)); }
		}


		public Brush TextForeground
		{
			get { return AppSettings.TextForeground; }
			set { AppSettings.TextForeground = value as SolidColorBrush; OnPropertyChanged(nameof(TextForeground)); }
		}

		public Brush TextBackground
		{
			get { return AppSettings.TextBackground; }
			set { AppSettings.TextBackground = value as SolidColorBrush; OnPropertyChanged(nameof(TextBackground)); }
		}

		public Brush SelectionBackground
		{
			get { return AppSettings.SelectionBackground; }
			set { AppSettings.SelectionBackground = value as SolidColorBrush; OnPropertyChanged(nameof(SelectionBackground)); }
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
