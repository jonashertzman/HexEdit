using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace HexEdit;

public class MainWindowViewModel : INotifyPropertyChanged
{

	#region Properties

	public string WindowTitle
	{
		get { return string.IsNullOrEmpty(CurrentFile) ? Title : $"{Title} - {CurrentFile}"; }
	}

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
			DateTime buildDate = new FileInfo(Environment.ProcessPath).LastWriteTime;
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
		set
		{
			AppSettings.FontSize = value;
			Zoom = 0;
			OnPropertyChangedRepaint(nameof(FontSize)); OnPropertyChanged(nameof(ZoomedFontSize));
		}
	}

	public int Zoom
	{
		get { return AppSettings.Zoom; }
		set { AppSettings.Zoom = Math.Max(value, 1 - FontSize); OnPropertyChanged(nameof(Zoom)); OnPropertyChanged(nameof(ZoomedFontSize)); }
	}

	public int ZoomedFontSize
	{
		get { return Math.Max(FontSize + Zoom, 1); }
	}


	public string CurrentFile
	{
		get;
		set { field = value; OnPropertyChanged(nameof(CurrentFile)); OnPropertyChanged(nameof(WindowTitle)); }
	}

	public Encoding FilePreview
	{
		get;
		set { field = value; OnPropertyChanged(nameof(FilePreview)); OnPropertyChanged(nameof(SelectedPreview)); }
	}

	public int SelectedPreview
	{
		get { return (int)FilePreview; }
		set { FilePreview = (Encoding)value; }
	}

	public ObservableCollection<byte> FileContent
	{
		get;
		set { field = value; Chunks = []; OnPropertyChanged(nameof(FileContent)); }
	} = [];

	public ObservableCollection<Chunk> Chunks
	{
		get;
		set { field = value; OnPropertyChanged(nameof(Chunks)); }
	} = [];


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

	public int UpdateTrigger
	{
		get;
		set { field = value; OnPropertyChanged(nameof(UpdateTrigger)); }
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
