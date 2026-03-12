using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace HexEdit;

public partial class MainWindow : Window
{

	#region Members

	MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();

	readonly Dictionary<int, UnicodeInfo> codePointCache = [];

	static readonly HttpClient client = new();

	#endregion

	#region Constructor

	public MainWindow()
	{
		InitializeComponent();

		PreviewModeCombobox.ItemsSource = Enum.GetValues<Encoding>();

		DataContext = ViewModel;

		Log.OwnerWindow = this;
	}

	#endregion

	#region Methods

	private void LoadSettings()
	{
		AppSettings.LoadSettings();

		if (Utils.PointIsOnScreen(AppSettings.PositionLeft, AppSettings.PositionTop) || Utils.PointIsOnScreen(AppSettings.PositionLeft + AppSettings.Width, AppSettings.PositionTop))
		{
			this.Left = AppSettings.PositionLeft;
			this.Top = AppSettings.PositionTop;
			this.Width = AppSettings.Width;
			this.Height = AppSettings.Height;
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
		Mouse.OverrideCursor = Cursors.Wait;

		try
		{
			byte[] bytes = File.ReadAllBytes(path);

			ViewModel.CurrentFile = path;
			ViewModel.FileContent = new ObservableCollection<byte>(bytes);

			Encoding foundEncoding = FileEncoding.DetectEncoding(bytes);

			ViewModel.SelectedPreview = (int)foundEncoding;
			ViewModel.Chunks = new(FileEncoding.ParseDataAs(foundEncoding, bytes));
		}
		catch (Exception exception)
		{
			Mouse.OverrideCursor = null;

			//MessageBox.Show(exception.Message, $"Error Opening File {path}", MessageBoxButton.OK, MessageBoxImage.Error);
			StyledMessageBox.Show(exception.Message, $"Error Opening File {path}");
		}

		Mouse.OverrideCursor = null;
	}

	private async Task<UnicodeInfo> GetCharacterInfo(int codePoint)
	{
		// Check if we have the character info cached in memory.
		if (codePointCache.TryGetValue(codePoint, out UnicodeInfo unicodeInfo))
		{
			return unicodeInfo;
		}

		// If not, check if we have it cached on disk.
		if (ReadCharacterInfo(codePoint, out unicodeInfo))
		{
			codePointCache[codePoint] = unicodeInfo;
			return unicodeInfo;
		}

		// If not, fetch it from the API.
		//if (FetchCharacterInfo(codePoint, out unicodeInfo))
		//{
		//	codePointCache[codePoint] = unicodeInfo;
		//	return unicodeInfo;
		//}

		// If that fails, read it from the fallback resource file.
		if (ReadCharacterInfoFallback(codePoint, out unicodeInfo))
		{
			codePointCache[codePoint] = unicodeInfo;
			return unicodeInfo;
		}

		return null;
	}

	private bool ReadCharacterInfo(int codePoint, out UnicodeInfo unicodeInfo)
	{
		string path = Path.Combine(AppSettings.CodePointDirectory, $"{codePoint}.json");

		if (File.Exists(path))
		{
			try
			{
				unicodeInfo = JsonSerializer.Deserialize<UnicodeInfo>(File.ReadAllText(path));
				if (unicodeInfo != null)
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.LogUnhandledException(ex, $"Failed to read cached character info for code point {codePoint} from disk.", true);
			}
		}

		unicodeInfo = null;
		return false;
	}

	private bool FetchCharacterInfo(int codePoint, out UnicodeInfo unicodeInfo)
	{
		unicodeInfo = null;
		try
		{
			HttpResponseMessage response = client.GetAsync($"https://ucdapi.org/unicode/10.0.0/codepoint/dec/{codePoint}").Result;
			if (response.IsSuccessStatusCode)
			{
				unicodeInfo = response.Content.ReadAsAsync<UnicodeInfo>().Result;

				Directory.CreateDirectory(AppSettings.CodePointDirectory);
				File.WriteAllText(Path.Combine(AppSettings.CodePointDirectory, $"{codePoint}.json"), JsonSerializer.Serialize(unicodeInfo));

				return true;
			}
		}
		catch (Exception) { }

		return false;
	}

	private bool ReadCharacterInfoFallback(int codePoint, out UnicodeInfo unicodeInfo)
	{
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HexEdit.Resources.UnicodeCharacters.txt");

		if (stream != null)
		{
			using StreamReader reader = new(stream);

			while (reader.ReadLine() is string line)
			{
				if (line.StartsWith('#'))
				{
					continue;
				}
				else
				{
					string[] lineSegments = line.Split(';');
					if (lineSegments.Length == 2)
					{
						if (lineSegments[0].Contains(".."))
						{
							string[] codePointRange = lineSegments[0].Split("..");
							int rangeStart = Convert.ToInt32(codePointRange[0].Trim(), 16);
							int rangeEnd = Convert.ToInt32(codePointRange[1].Trim(), 16);

							if (codePoint >= rangeStart && codePoint <= rangeEnd)
							{
								unicodeInfo = new UnicodeInfo() { codePoint = codePoint, name = lineSegments[1].Trim().Replace("*", codePoint.ToString("X4")) };
								return true;
							}
						}
						else
						{
							if (Convert.ToInt32(lineSegments[0].Trim(), 16) == codePoint)
							{
								unicodeInfo = new UnicodeInfo() { codePoint = codePoint, name = lineSegments[1].Trim() };
								return true;
							}
						}
					}
				}
			}
		}

		unicodeInfo = null;
		return false;
	}

	private async void CheckForNewVersion(bool forced = false)
	{
		if (AppSettings.CheckForUpdates && AppSettings.LastUpdateTime < DateTime.Now.AddDays(-5) || forced)
		{
			try
			{
				Debug.Print("Checking for new version...");

				HttpClient httpClient = new();
				string result = await httpClient.GetStringAsync("https://jonashertzman.github.io/HexEdit/download/version.txt");

				Debug.Print($"Latest version found: {result}");
				ViewModel.NewBuildAvailable = int.Parse(result) > int.Parse(ViewModel.BuildNumber);
			}
			catch (Exception exception)
			{
				Debug.Print($"Version check failed: {exception.Message}");
			}

			AppSettings.LastUpdateTime = DateTime.Now;
		}
	}

	#endregion

	#region Events

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		this.WindowState = AppSettings.WindowState;
	}

	private void Window_Initialized(object sender, EventArgs e)
	{
		LoadSettings();

		CheckForNewVersion();
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
		ViewModel.Chunks = new(FileEncoding.ParseDataAs((Encoding)PreviewModeCombobox.SelectedItem, [.. ViewModel.FileContent]));
	}

	private async void Preview_SelectionChanged(object sender, ChunkEventArgs e)
	{
		if (e.SelectedItem != null)
		{
			Chunk c = e.SelectedItem;

			if (c != null)
			{
				TextBoxChunkTitle.Text = c.Description;

				if (c.UnicodeCharacter != -1)
				{
					TextBoxChunkValue.Text = c.UnicodeCharacter.ToString("X4");
					TextBoxChunkInfo.Text = "";

					UnicodeInfo info = await GetCharacterInfo(c.UnicodeCharacter);
					if (info != null)
					{
						TextBoxChunkInfo.Text = $"""
						{info.name}
						{info.block}
						{info.generalCategory}
						{info.script}
						""";
					}
					else
					{
						TextBoxChunkInfo.Text = "N/A";
					}
				}
			}
		}
	}

	private void LightMode_Click(object sender, RoutedEventArgs e)
	{
		ViewModel.Theme = Themes.Light;
	}

	private void DarkMode_Click(object sender, RoutedEventArgs e)
	{
		ViewModel.Theme = Themes.Dark;
	}

	private void Hyperlink_OpenHomepage(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
		e.Handled = true;
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

		var oldDarkTheme = AppSettings.DarkTheme.Clone();
		var oldLightTheme = AppSettings.LightTheme.Clone();
		var oldTheme = ViewModel.Theme;

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

			AppSettings.DarkTheme = oldDarkTheme;
			AppSettings.LightTheme = oldLightTheme;
			ViewModel.Theme = oldTheme;
		}
	}

	private void CommandAbout_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		CheckForNewVersion(true);

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
