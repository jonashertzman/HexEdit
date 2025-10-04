using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace HexEdit;

public static class AppSettings
{

	#region Members

	private static readonly SettingsData Settings = new();

	private static readonly string AppDataDirectory = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HexEdit");
	public static readonly string SettingsPath = Path.Combine(AppDataDirectory, "Settings.xml");
	public static readonly string LogPath = Path.Combine(AppDataDirectory, "HexEdit.log");
	public static readonly string CodePointDirectory = Path.Combine(AppDataDirectory, "CodePoints");

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

	public static Themes Theme
	{
		get { return Settings.Theme; }
		set
		{
			Settings.Theme = value;

			UpdateCachedSettings();
			//NotifyStaticPropertyChanged(nameof(WindowForegroundColor));
			//NotifyStaticPropertyChanged(nameof(WindowBackgroundColor));
			//NotifyStaticPropertyChanged(nameof(DisabledForegroundColor));
			//NotifyStaticPropertyChanged(nameof(DialogBackgroundColor));
			//NotifyStaticPropertyChanged(nameof(ControlLightBackgroundColor));
			//NotifyStaticPropertyChanged(nameof(BorderForegroundColor));
		}
	}

	public static ColorTheme DarkTheme
	{
		get { return Settings.DarkTheme; }
		set { Settings.DarkTheme = value; }
	}

	public static ColorTheme LightTheme
	{
		get { return Settings.LightTheme; }
		set { Settings.LightTheme = value; }
	}

	public static ColorTheme CurrentTheme
	{
		get
		{
			return Theme switch
			{
				Themes.Light => Settings.LightTheme,
				Themes.Dark => Settings.DarkTheme,
				_ => throw new NotImplementedException(),
			};
		}
	}


	// Editor colors
	public static SolidColorBrush TextBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.TextBackground = value.Color.ToString();
		}
	}

	public static SolidColorBrush TextForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.TextForeground = value.Color.ToString();
		}
	}

	public static SolidColorBrush SelectionBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.SelectionBackground = value.Color.ToString();
		}
	}


	// UI colors
	public static SolidColorBrush WindowForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.NormalText = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(WindowForeground));
			NotifyStaticPropertyChanged(nameof(WindowForegroundColor));
		}
	} = DefaultSettings.LightTheme.NormalText.ToBrush();
	public static Color WindowForegroundColor
	{
		get { return WindowForeground.Color; }
	}

	public static SolidColorBrush DisabledForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.DisabledText = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(DisabledForeground));
			NotifyStaticPropertyChanged(nameof(DisabledForegroundColor));
		}
	} = DefaultSettings.LightTheme.DisabledText.ToBrush();
	public static Color DisabledForegroundColor
	{
		get { return DisabledForeground.Color; }
	}

	public static SolidColorBrush DisabledBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.DisabledBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(DisabledBackground));
			NotifyStaticPropertyChanged(nameof(DisabledBackgroundColor));
		}
	} = DefaultSettings.LightTheme.DisabledBackground.ToBrush();
	public static Color DisabledBackgroundColor
	{
		get { return DisabledBackground.Color; }
	}

	public static SolidColorBrush WindowBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.WindowBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(WindowBackground));
			NotifyStaticPropertyChanged(nameof(WindowBackgroundColor));
		}
	} = DefaultSettings.LightTheme.WindowBackground.ToBrush();
	public static Color WindowBackgroundColor
	{
		get { return WindowBackground.Color; }
	}

	public static SolidColorBrush DialogBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.DialogBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(DialogBackground));
			NotifyStaticPropertyChanged(nameof(DialogBackgroundColor));
		}
	} = DefaultSettings.LightTheme.DialogBackground.ToBrush();
	public static Color DialogBackgroundColor
	{
		get { return DialogBackground.Color; }
	}

	public static SolidColorBrush ControlLightBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.ControlLightBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(ControlLightBackground));
			NotifyStaticPropertyChanged(nameof(ControlLightBackgroundColor));
		}
	} = DefaultSettings.LightTheme.ControlLightBackground.ToBrush();
	public static Color ControlLightBackgroundColor
	{
		get { return ControlLightBackground.Color; }
	}

	public static SolidColorBrush ControlDarkBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.ControlDarkBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(ControlDarkBackground));
			NotifyStaticPropertyChanged(nameof(ControlDarkBackgroundColor));
		}
	} = DefaultSettings.LightTheme.ControlDarkBackground.ToBrush();
	public static Color ControlDarkBackgroundColor
	{
		get { return ControlDarkBackground.Color; }
	}

	public static SolidColorBrush BorderForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.BorderLight = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(BorderForeground));
			NotifyStaticPropertyChanged(nameof(BorderForegroundColor));
		}
	} = DefaultSettings.LightTheme.BorderLight.ToBrush();
	public static Color BorderForegroundColor
	{
		get { return BorderForeground.Color; }
	}

	public static SolidColorBrush BorderDarkForeground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.BorderDark = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(BorderDarkForeground));
			NotifyStaticPropertyChanged(nameof(BorderDarkForegroundColor));
		}
	} = DefaultSettings.LightTheme.BorderDark.ToBrush();
	public static Color BorderDarkForegroundColor
	{
		get { return BorderDarkForeground.Color; }
	}

	public static SolidColorBrush HighlightBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.HighlightBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(HighlightBackground));
			NotifyStaticPropertyChanged(nameof(HighlightBackgroundColor));
		}
	} = DefaultSettings.LightTheme.HighlightBackground.ToBrush();
	public static Color HighlightBackgroundColor
	{
		get { return HighlightBackground.Color; }
	}

	public static SolidColorBrush HighlightBorder
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.HighlightBorder = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(HighlightBorder));
			NotifyStaticPropertyChanged(nameof(HighlightBorderColor));
		}
	} = DefaultSettings.LightTheme.HighlightBorder.ToBrush();
	public static Color HighlightBorderColor
	{
		get { return HighlightBorder.Color; }
	}

	public static SolidColorBrush AttentionBackground
	{
		get;
		set
		{
			field = value;
			field.Freeze();
			CurrentTheme.AttentionBackground = value.Color.ToString();
			NotifyStaticPropertyChanged(nameof(AttentionBackground));
			NotifyStaticPropertyChanged(nameof(AttentionBackgroundColor));
		}
	} = DefaultSettings.LightTheme.AttentionBackground.ToBrush();
	public static Color AttentionBackgroundColor
	{
		get { return AttentionBackground.Color; }
	}


	#endregion

	#region Methods

	internal static void LoadSettings()
	{
		SettingsData storedSettings = ReadSettingsFromDisk();

		if (storedSettings != null)
		{
			MergeSettings(Settings, storedSettings);
		}

		UpdateCachedSettings();
	}

	private static void MergeSettings(object source, object addition)
	{
		foreach (var property in addition.GetType().GetProperties())
		{
			if (property.PropertyType.Name == nameof(ColorTheme))
			{
				MergeSettings(property.GetValue(source), property.GetValue(addition));
			}
			else
			{
				if (property.GetValue(addition) != null)
				{
					property.SetValue(source, property.GetValue(addition));
				}
			}
		}
	}

	private static SettingsData ReadSettingsFromDisk()
	{
		DataContractSerializer xmlSerializer = new(typeof(SettingsData));

		if (File.Exists(SettingsPath))
		{
			using var xmlReader = XmlReader.Create(SettingsPath);
			try
			{
				return (SettingsData)xmlSerializer.ReadObject(xmlReader);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error Parsing Settings File", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		return null;
	}

	internal static void WriteSettingsToDisk()
	{
		try
		{
			DataContractSerializer xmlSerializer = new(typeof(SettingsData));
			var xmlWriterSettings = new XmlWriterSettings { Indent = true, IndentChars = " " };

			Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));

			using XmlWriter xmlWriter = XmlWriter.Create(SettingsPath, xmlWriterSettings);

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

		TextForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.TextForeground));
		TextBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.TextBackground));

		SelectionBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.SelectionBackground));

		// UI colors
		WindowForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.NormalText));
		DisabledForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DisabledText));

		DisabledBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DisabledBackground));

		WindowBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.WindowBackground));
		DialogBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.DialogBackground));

		ControlLightBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.ControlLightBackground));
		ControlDarkBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.ControlDarkBackground));

		BorderForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.BorderLight));
		BorderDarkForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.BorderDark));

		HighlightBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HighlightBackground));
		HighlightBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.HighlightBorder));

		AttentionBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(CurrentTheme.AttentionBackground));
	}

	#endregion

	#region NotifyStaticPropertyChanged

	public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

	private static void NotifyStaticPropertyChanged(string propertyName)
	{
		StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
	}

	#endregion

}
