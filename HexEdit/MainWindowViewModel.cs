using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace HexEdit;

public class MainWindowViewModel : INotifyPropertyChanged
{

	#region Properties

	public string Title
	{
		get { return "Hex Edit"; }
	}

	public string Version
	{
		get { return "0.1"; }
	}

	public string BuildNumber
	{
		get
		{
			DateTime buildDate = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).LastWriteTime;
			return $"{buildDate:yy}{buildDate.DayOfYear:D3}";
		}
	}

	public string ApplicationName
	{
		get { return $"{Title} {Version}"; }
	}

	public string FullApplicationName
	{
		get { return $"{Title} {Version} (Build {BuildNumber})"; }
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


	public int BytesPerRow
	{
		get { return AppSettings.BytesPerRow; }
		set { AppSettings.BytesPerRow = value; OnPropertyChangedRepaint(nameof(BytesPerRow)); OnPropertyChanged(nameof(BytesPerRowValue)); }
	}

	public string BytesPerRowValue
	{
		get { return BytesPerRow.ToString(); }
		set { BytesPerRow = int.Parse(value); }
	}


	public Brush TextForeground
	{
		get { return AppSettings.TextForeground; }
		set { AppSettings.TextForeground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(TextForeground)); }
	}

	public Brush TextBackground
	{
		get { return AppSettings.TextBackground; }
		set { AppSettings.TextBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(TextBackground)); }
	}

	public Brush SelectionBackground
	{
		get { return AppSettings.SelectionBackground; }
		set { AppSettings.SelectionBackground = value as SolidColorBrush; OnPropertyChangedRepaint(nameof(SelectionBackground)); }
	}


	int updateTrigger;
	public int UpdateTrigger
	{
		get { return updateTrigger; }
		set { updateTrigger = value; OnPropertyChanged(nameof(UpdateTrigger)); }
	}

	#endregion

	#region INotifyPropertyChanged

	public event PropertyChangedEventHandler PropertyChanged;

	public void OnPropertyChangedRepaint(string name)
	{
		UpdateTrigger++;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	public void OnPropertyChanged(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	#endregion

}
