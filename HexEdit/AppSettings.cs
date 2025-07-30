using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace HexEdit;

public static class AppSettings
{

	#region Members

	private const string SETTINGS_DIRECTORY = "HexEdit";
	private const string SETTINGS_FILE_NAME = "Settings.xml";

	private static SettingsData Settings = new();

	#endregion

	#region Properties

	public static string Id
	{
		get { return Settings.Id; }
	}

	public static double PositionLeft
	{
		get { return Settings.PositionLeft; }
		set { Settings.PositionLeft = value; }
	}

	public static double PositionTop
	{
		get { return Settings.PositionTop; }
		set { Settings.PositionTop = value; }
	}

	public static double Width
	{
		get { return Settings.Width; }
		set { Settings.Width = value; }
	}

	public static double Height
	{
		get { return Settings.Height; }
		set { Settings.Height = value; }
	}

	public static WindowState WindowState
	{
		get { return Settings.WindowState; }
		set { Settings.WindowState = value; }
	}

	public static FontFamily Font
	{
		get;
		set
		{
			field = value;
			Settings.Font = value.ToString();
		}
	}

	public static int FontSize
	{
		get { return Settings.FontSize; }
		set { Settings.FontSize = value; }
	}

	public static int Zoom
	{
		get { return Settings.Zoom; }
		set { Settings.Zoom = value; }
	}

	public static int BytesPerRow
	{
		get { return Settings.BytesPerRow; }
		set { Settings.BytesPerRow = value; }
	}

	public static SolidColorBrush TextForeground
	{
		get;
		set { field = value; Settings.TextForeground = value.Color; }
	}

	public static SolidColorBrush TextBackground
	{
		get;
		set { field = value; Settings.TextBackground = value.Color; }
	}

	public static SolidColorBrush SelectionBackground
	{
		get;
		set { field = value; Settings.SelectionBackground = value.Color; }
	}

	#endregion

	#region Methods

	internal static void ReadSettingsFromDisk()
	{
		string settingsPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY), SETTINGS_FILE_NAME);
		DataContractSerializer xmlSerializer = new(typeof(SettingsData));

		if (File.Exists(settingsPath))
		{
			using XmlReader xmlReader = XmlReader.Create(settingsPath);
			try
			{
				Settings = (SettingsData)xmlSerializer.ReadObject(xmlReader);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error Parsing XML", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		if (Settings == null)
		{
			Settings = new SettingsData();
		}

		UpdateCachedSettings();
	}

	internal static void WriteSettingsToDisk()
	{
		try
		{
			string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY);

			DataContractSerializer xmlSerializer = new(typeof(SettingsData));
			var xmlWriterSettings = new XmlWriterSettings { Indent = true, IndentChars = " " };

			if (!Directory.Exists(settingsPath))
			{
				Directory.CreateDirectory(settingsPath);
			}

			using XmlWriter xmlWriter = XmlWriter.Create(Path.Combine(settingsPath, SETTINGS_FILE_NAME), xmlWriterSettings);
			xmlSerializer.WriteObject(xmlWriter, Settings);
		}
		catch (Exception e)
		{
			MessageBox.Show(e.Message, "Error Saving Settings", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private static void UpdateCachedSettings()
	{
		Font = new FontFamily(Settings.Font);

		TextForeground = new SolidColorBrush(Settings.TextForeground);
		TextBackground = new SolidColorBrush(Settings.TextBackground);

		SelectionBackground = new SolidColorBrush(Settings.SelectionBackground);
	}

	#endregion

}
