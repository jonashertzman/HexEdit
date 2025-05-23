﻿using System.IO;
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

	private static SettingsData Settings = new SettingsData();

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

	private static FontFamily font;
	public static FontFamily Font
	{
		get { return font; }
		set
		{
			font = value;
			Settings.Font = value.ToString();
		}
	}

	public static int FontSize
	{
		get { return Settings.FontSize; }
		set { Settings.FontSize = value; }
	}

	public static int BytesPerRow
	{
		get { return Settings.BytesPerRow; }
		set { Settings.BytesPerRow = value; }
	}

	private static SolidColorBrush textForeground;
	public static SolidColorBrush TextForeground
	{
		get { return textForeground; }
		set { textForeground = value; Settings.TextForeground = value.Color; }
	}

	private static SolidColorBrush textBackground;
	public static SolidColorBrush TextBackground
	{
		get { return textBackground; }
		set { textBackground = value; Settings.TextBackground = value.Color; }
	}

	private static SolidColorBrush selectionBackground;
	public static SolidColorBrush SelectionBackground
	{
		get { return selectionBackground; }
		set { selectionBackground = value; Settings.SelectionBackground = value.Color; }
	}

	#endregion

	#region Methods

	internal static void ReadSettingsFromDisk()
	{
		string settingsPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIRECTORY), SETTINGS_FILE_NAME);
		DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(SettingsData));

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

			DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(SettingsData));
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
