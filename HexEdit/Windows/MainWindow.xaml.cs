using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace HexEdit;

public partial class MainWindow : Window
{

	#region Members

	MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();

	readonly byte[] UTF8_BOM = [0xEF, 0xBB, 0xBF];
	readonly byte[] UTF32LE_BOM = [0xFF, 0xFE, 0x00, 0x00];
	readonly byte[] UTF32BE_BOM = [0x00, 0x00, 0xFE, 0xFF];
	readonly byte[] UTF16LE_BOM = [0xFF, 0xFE];
	readonly byte[] UTF16BE_BOM = [0xFE, 0xFF];

	readonly Dictionary<int, UnicodeInfo> characterInfo = [];

	static readonly HttpClient client = new();

	#endregion

	#region Constructor

	public MainWindow()
	{
		InitializeComponent();

		PreviewModeCombobox.ItemsSource = Enum.GetValues(typeof(Encoding));

		DataContext = ViewModel;
	}

	#endregion

	#region Methods

	private void LoadSettings()
	{
		AppSettings.LoadSettings();

		this.Left = AppSettings.PositionLeft;
		this.Top = AppSettings.PositionTop;
		this.Width = AppSettings.Width;
		this.Height = AppSettings.Height;
		this.WindowState = AppSettings.WindowState;

		Task.Run(() => LoadUnicodeInfoCache());
	}

	private void LoadUnicodeInfoCache()
	{
		Directory.CreateDirectory(AppSettings.CodePointDirectory);
		foreach (var file in Directory.GetFiles(AppSettings.CodePointDirectory, "*.json"))
		{
			try
			{
				var info = JsonSerializer.Deserialize<UnicodeInfo>(File.ReadAllText(file));
				if (info != null)
				{
					characterInfo[info.codePoint] = info;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to load code point info from disk: " + ex.Message);
			}
		}
	}

	private void SaveSettings()
	{
		AppSettings.PositionLeft = this.Left;
		AppSettings.PositionTop = this.Top;
		AppSettings.Width = this.Width;
		AppSettings.Height = this.Height;
		AppSettings.WindowState = this.WindowState;

		AppSettings.WriteSettingsToDisk();
	}

	private void OpenFile(string path)
	{
		try
		{
			byte[] bytes = File.ReadAllBytes(path);


			ViewModel.CurrentFile = path;
			ViewModel.FileContent = new ObservableCollection<byte>(bytes);

			Encoding foundEncoding = DetectEncoding(bytes);

			ViewModel.SelectedPreview = (int)foundEncoding;
			ParseFileAs(foundEncoding);
		}
		catch (Exception exception)
		{
			MessageBox.Show(exception.Message, $"Error Opening File {path}", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private void ParseFileAs(Encoding foundEncoding)
	{
		switch (foundEncoding)
		{
			case Encoding.Utf8:
				ParseUtf8([.. ViewModel.FileContent]);
				break;

			case Encoding.Utf16le:
				ParseUtf16le([.. ViewModel.FileContent]);
				break;

			case Encoding.Utf16be:
				ParseUtf16be([.. ViewModel.FileContent]);
				break;

			case Encoding.Utf32le:
				ParseUtf32le([.. ViewModel.FileContent]);
				break;

			case Encoding.Utf32be:
				ParseUtf32be([.. ViewModel.FileContent]);
				break;

			default:
				ParseDefault([.. ViewModel.FileContent]);
				break;
		}
	}

	private void ParseDefault(byte[] bytes)
	{
		ViewModel.Chunks = [];
	}

	private void ParseUtf8(byte[] bytes)
	{
		ObservableCollection<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF8_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF8_BOM));
			i += UTF8_BOM.Length;
		}

		while (i < bytes.Length)
		{
			// 1 byte character
			int end = i + 1;
			if (bytes[i] > 0x00 && bytes[i] <= 0x7F)
			{
				chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
				i++;
				continue;
			}

			// 2 byte character
			end = i + 2;
			//if (bytes.Length <= end) return false;

			if (bytes[i] >= 0xC2 && bytes[i] <= 0xDF)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
					i += 2;
					continue;
				}
			}

			// 3 byte character
			end = i + 3;
			//		if (bytes.Length <= end) return false;

			if (bytes[i] == 0xE0)
			{
				if (bytes[i + 1] >= 0xA0 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] >= 0xE1 && bytes[i] <= 0xEC)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] == 0xED)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x9F)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] >= 0xEE && bytes[i] <= 0xEF)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
						i += 3;
						continue;
					}
				}
			}

			// 4 byte character
			end = i + 4;
			//		if (bytes.Length <= end) return false;


			if (bytes[i] == 0xF0)
			{
				if (bytes[i + 1] >= 0x90 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
							i += 4;
							continue;
						}
					}
				}
			}

			if (bytes[i] >= 0xF1 && bytes[i] <= 0xF3)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
							i += 4;
							continue;
						}
					}
				}
			}

			if (bytes[i] == 0xF4)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x8F)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
							i += 4;
							continue;
						}
					}
				}
			}

			i++;
		}

		ViewModel.Chunks = chunks;

	}

	private void ParseUtf16le(byte[] bytes)
	{
		ObservableCollection<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF16LE_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF16LE_BOM));
			i += UTF16LE_BOM.Length;
		}

		for (; i < bytes.Length - 1; i += 2)
		{
			if (char.IsHighSurrogate((char)(bytes[i + 1] << 8 | bytes[i])))
			{
				chunks.Add(new Chunk(ChunkType.Utf16leCharacter, i, bytes[i..(i + 4)]));
				i += 2;
			}
			else
			{
				chunks.Add(new Chunk(ChunkType.Utf16leCharacter, i, bytes[i..(i + 2)]));
			}
		}

		ViewModel.Chunks = chunks;
	}

	private void ParseUtf16be(byte[] bytes)
	{
		ObservableCollection<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF16BE_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF16BE_BOM));
			i += UTF16BE_BOM.Length;
		}

		for (; i < bytes.Length - 1; i += 2)
		{
			if (char.IsHighSurrogate((char)(bytes[i] << 8 | bytes[i + 1])))
			{
				chunks.Add(new Chunk(ChunkType.Utf16beCharacter, i, bytes[i..(i + 4)]));
				i += 2;
			}
			else
			{
				chunks.Add(new Chunk(ChunkType.Utf16beCharacter, i, bytes[i..(i + 2)]));
			}
		}

		ViewModel.Chunks = chunks;
	}

	private void ParseUtf32le(byte[] bytes)
	{
		ObservableCollection<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF32LE_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF32LE_BOM));
			i += UTF32LE_BOM.Length;
		}

		for (; i < bytes.Length - 3; i += 4)
		{
			chunks.Add(new Chunk(ChunkType.Utf32leCharacter, i, bytes[i..(i + 4)]));
		}

		ViewModel.Chunks = chunks;
	}

	private void ParseUtf32be(byte[] bytes)
	{
		ObservableCollection<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF32BE_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF32BE_BOM));
			i += UTF32BE_BOM.Length;
		}

		for (; i < bytes.Length - 3; i += 4)
		{
			chunks.Add(new Chunk(ChunkType.Utf32beCharacter, i, bytes[i..(i + 4)]));
		}

		ViewModel.Chunks = chunks;
	}

	private Encoding DetectEncoding(byte[] bytes)
	{
		// Check if the file has a BOM
		if (bytes.StartsWith(UTF8_BOM))
		{
			return Encoding.Utf8;
		}
		else if (bytes.StartsWith(UTF32LE_BOM)) // Must check this before UTF16 since the first 2 bytes are the same as an UTF16 little endian BOM.
		{
			return Encoding.Utf32le;
		}
		else if (bytes.StartsWith(UTF32BE_BOM))
		{
			return Encoding.Utf32be;
		}
		else if (bytes.StartsWith(UTF16LE_BOM))
		{
			return Encoding.Utf16le;
		}
		else if (bytes.StartsWith(UTF16BE_BOM))
		{
			return Encoding.Utf16be;
		}

		// No bom found, check if data passes as a bom-less UTF-8 file.
		if (CheckValidUtf8(bytes))
		{
			return Encoding.Utf8;
		}

		// Check if data could be a bom-less UTF-16 or UTF-32 file.
		//for (int i = 0; i < bytes.Length; i++)
		//{
		//	if (bytes[i] == 0)
		//	{
		//		if (i % 2 == 1) // Little endian since the null byte IS NOT on a multiple of 2 or 4.
		//		{
		//			if (i < bytes.Length && bytes[i + 1] == 0) // UTF-16 cannot have 2 consecutive null bytes, must be UTF-32.
		//			{
		//				return Encoding.Utf32le;
		//			}
		//			else
		//			{
		//				return Encoding.Utf32le;
		//			}
		//		}
		//		else // Big endian since the null byte IS on a multiple of 2 or 4.
		//		{
		//			if (i < bytes.Length && bytes[i + 1] == 0) // UTF-16 cannot have 2 consecutive null bytes, must be UTF-32.
		//			{
		//				return Encoding.Utf32be;
		//			}
		//			else
		//			{
		//				return Encoding.Utf32be;
		//			}
		//		}
		//	}
		//}

		return Encoding.Unknown;
	}

	private bool CheckValidUtf8(byte[] bytes)
	{
		int i = 0;

		if (bytes.StartsWith(UTF8_BOM))
		{
			i += UTF8_BOM.Length;
		}

		while (i < bytes.Length)
		{
			// 1 byte character
			if (bytes[i] >= 0x00 && bytes[i] <= 0x7F)
			{
				i++;
				continue;
			}

			// 2 byte character
			if (bytes[i] >= 0xC2 && bytes[i] <= 0xDF)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					i += 2;
					continue;
				}
			}

			// 3 byte character
			if (bytes[i] == 0xE0)
			{
				if (bytes[i + 1] >= 0xA0 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] >= 0xE1 && bytes[i] <= 0xEC)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] == 0xED)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x9F)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] >= 0xEE && bytes[i] <= 0xEF)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						i += 3;
						continue;
					}
				}
			}

			// 4 byte character
			if (bytes[i] == 0xF0)
			{
				if (bytes[i + 1] >= 0x90 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							i += 4;
							continue;
						}
					}
				}
			}

			if (bytes[i] >= 0xF1 && bytes[i] <= 0xF3)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							i += 4;
							continue;
						}
					}
				}
			}

			if (bytes[i] == 0xF4)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x8F)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							i += 4;
							continue;
						}
					}
				}
			}

			return false;
		}
		return true;
	}

	#endregion

	#region Events

	private void Window_Initialized(object sender, EventArgs e)
	{
		LoadSettings();
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		SaveSettings();
	}

	private void Window_ContentRendered(object sender, EventArgs e)
	{
		if (Environment.GetCommandLineArgs().Length > 1)
		{
			OpenFile(Environment.GetCommandLineArgs()[1]);
		}

		Preview.Focus();
	}

	private void Grid_PreviewDrop(object sender, DragEventArgs e)
	{
		var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

		if (paths?.Length > 0)
		{
			OpenFile(paths[0]);
		}
	}

	private void Grid_PreviewDragOver(object sender, DragEventArgs e)
	{
		e.Effects = DragDropEffects.Move;
		e.Handled = true;
	}

	private void Preview_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		bool controlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

		if (controlPressed)
		{
			ViewModel.Zoom += e.Delta / Math.Abs(e.Delta);
		}
		else
		{
			int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
			VerticalScrollbar.Value -= lines;
		}
	}

	private void PreviewModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
	{
		ParseFileAs((Encoding)PreviewModeCombobox.SelectedItem);
	}

	private async void Preview_SelectionChanged(object sender, ChunkEventArgs e)
	{
		if (e.SelectedItem != null)
		{
			Chunk c = e.SelectedItem;

			if (c != null)
			{
				if (c.Type == ChunkType.Bom)
				{

				}
				else if (c.UnicodeCharacter != -1)
				{
					UnicodeInfo info = await GetCharacterInfo(c);

					ChunkTitle.Text = info.codePoint.ToString("X4");
					ChunkInfo.Text = $"""
						{info.name}
						{info.block}
						{info.generalCategory}
						{info.script}
						""";
				}
			}
		}
	}

	private async Task<UnicodeInfo> GetCharacterInfo(Chunk c)
	{
		if (characterInfo.TryGetValue(c.UnicodeCharacter, out UnicodeInfo info))
		{
			return info;
		}
		else
		{
			Debug.WriteLine($"Fetching unicode info for character {c.UnicodeCharacter}");

			info = null;
			HttpResponseMessage response = await client.GetAsync($"https://ucdapi.org/unicode/10.0.0/codepoint/dec/{c.UnicodeCharacter}");
			if (response.IsSuccessStatusCode)
			{
				info = await response.Content.ReadAsAsync<UnicodeInfo>();
				characterInfo[c.UnicodeCharacter] = info;

				_ = Task.Run(() =>
				{
					Directory.CreateDirectory(AppSettings.CodePointDirectory);
					File.WriteAllText(Path.Combine(AppSettings.CodePointDirectory, $"{c.UnicodeCharacter}.json"), JsonSerializer.Serialize(info));
				});
			}

			return info;
		}
	}

	#region Commands

	private void CommandNew_Executed(object sender, ExecutedRoutedEventArgs e)
	{

	}

	private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		OpenFileDialog openFileDialog = new();

		if (!string.IsNullOrEmpty(ViewModel.CurrentFile))
			openFileDialog.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(ViewModel.CurrentFile));

		if (openFileDialog.ShowDialog() == true)
		{
			OpenFile(openFileDialog.FileName);
		}
	}

	private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
	{

	}

	private void CommandSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void CommandExit_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		this.Close();
	}

	private void CommandOptions_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		// Store existing settings data in case the changes are canceled.
		var oldFont = ViewModel.Font;
		var oldFontSize = ViewModel.FontSize;
		var oldTextBackground = ViewModel.TextBackground;
		var oldTextForeground = ViewModel.TextForeground;
		var oldSelectionBackground = ViewModel.SelectionBackground;

		OptionsWindow optionsWindow = new() { DataContext = ViewModel, Owner = this };
		optionsWindow.ShowDialog();

		if (optionsWindow.DialogResult == true)
		{
			SaveSettings();
		}
		else
		{
			// Options window was canceled, revert to old settings.
			ViewModel.Font = oldFont;
			ViewModel.FontSize = oldFontSize;

			ViewModel.TextBackground = oldTextBackground;
			ViewModel.TextForeground = oldTextForeground;
			ViewModel.SelectionBackground = oldSelectionBackground;
		}
	}

	private void CommandAbout_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		AboutWindow aboutWindow = new() { Owner = this, DataContext = ViewModel };
		aboutWindow.ShowDialog();
	}

	private void CommandZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ViewModel.Zoom += 1;
	}

	private void CommandZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ViewModel.Zoom -= 1;
	}

	private void CommandResetZoom_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		ViewModel.Zoom = 0;
	}

	#endregion

	#endregion

}
