﻿using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace HexEdit;

public partial class ExceptionWindow : Window
{

	#region Constructor

	public ExceptionWindow()
	{
		InitializeComponent();
		DataContext = this;
	}

	#endregion

	#region Properties

	public string ExceptionType
	{
		get;
		set { field = value; OnPropertyChanged(ExceptionType); }
	}

	public string ExceptionMessage
	{
		get;
		set { field = value; OnPropertyChanged(ExceptionMessage); }
	}

	public string Source
	{
		get;
		set { field = value; OnPropertyChanged(Source); }
	}

	public string StackTrace
	{
		get;
		set { field = value; OnPropertyChanged(StackTrace); }
	}

	#endregion

	#region Events

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private async void ReportButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			HttpClient httpClient = new();

			CrashReportRequest cr = new()
			{
				ApplicationName = "FileDiff",
				BuildNumber = AppSettings.BuildNumber,
				ClientId = AppSettings.Id,
				ExceptionType = this.ExceptionType,
				ExceptionMessage = this.ExceptionMessage,
				Source = this.Source,
				StackTrace = this.StackTrace
			};

			string json = JsonSerializer.Serialize(cr);
			var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

			var response = await httpClient.PostAsync("https://localhost:7133/api/CrashReport", content);
		}
		catch (Exception ex)
		{
			Debug.Print($"Crash report failed: {ex.Message}");

			Log.LogUnhandledException(ex, ex.Source, true);
		}

		this.Close();
	}

	private void OpenLogFileButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			using Process p = new();

			p.StartInfo.FileName = AppSettings.LogPath;
			p.StartInfo.ErrorDialog = true;
			p.StartInfo.UseShellExecute = true;
			p.Start();
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Cannot open log file. {ex.Message}");
		}
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
