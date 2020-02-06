using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace HexEdit
{
	public partial class MainWindow : Window
	{

		#region Members

		MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();

		#endregion

		#region Constructor

		public MainWindow()
		{
			InitializeComponent();

			PreviewModeCombobox.ItemsSource = System.Enum.GetValues(typeof(PreviewMode));

			DataContext = ViewModel;
		}

		#endregion

		#region Methods

		private void LoadSettings()
		{
			AppSettings.ReadSettingsFromDisk();

			this.Left = AppSettings.PositionLeft;
			this.Top = AppSettings.PositionTop;
			this.Width = AppSettings.Width;
			this.Height = AppSettings.Height;
			this.WindowState = AppSettings.WindowState;
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
				ObservableCollection<Chunk> chunks = new ObservableCollection<Chunk>();

				// Check if the file has a BOM
				if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
				{
					ViewModel.FilePreview = PreviewMode.UTF8;
					chunks.Add(new Chunk(ChunkType.Bom, 0, bytes[0..3]));
					ValidUtf8(bytes, 3, ref chunks);
				}
				else if (bytes[0] == 0xFF && bytes[1] == 0xFE)
				{
					ViewModel.FilePreview = PreviewMode.UTF16LE;
				}
				else if (bytes[0] == 0xFE && bytes[1] == 0xFF)
				{
					ViewModel.FilePreview = PreviewMode.UTF16BE;
				}
				else if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
				{
					ViewModel.FilePreview = PreviewMode.UTF32LE;
				}

				// No bom found, check if data passes as a bom-less UTF-8 file
				else if (ValidUtf8(bytes, 0, ref chunks))
				{
					ViewModel.FilePreview = PreviewMode.UTF8;
				}

				ViewModel.CurrentFile = path;
				ViewModel.FileContent = new ObservableCollection<byte>(bytes);
				ViewModel.Chunks = chunks;
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, $"Error Opening File {path}", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private bool ValidUtf8(byte[] bytes, int offset, ref ObservableCollection<Chunk> chunks)
		{
			int i = offset;
			while (i < bytes.Length)
			{
				// 1 byte character
				int end = i + 1;
				if (bytes[i] >= 0x00 && bytes[i] <= 0x7F)
				{
					chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
					i++;
					continue;
				}

				// 2 byte character
				end = i + 2;
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

		private void Preview_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
		{
			int lines = SystemParameters.WheelScrollLines * e.Delta / 120;
			VerticalScrollbar.Value -= lines;
		}

		#region Commands

		private void CommandNew_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

		}

		private void CommandOpen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			if (openFileDialog.ShowDialog() == true)
			{
				OpenFile(openFileDialog.FileName);
			}
		}

		private void CommandSave_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

		}

		private void CommandSave_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CommandExit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void CommnadOptions_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			// Store existing settings data in case the changes are canceled.
			var oldFont = ViewModel.Font;
			var oldFontSize = ViewModel.FontSize;
			var oldTextBackground = ViewModel.TextBackground;
			var oldTextForeground = ViewModel.TextForeground;
			var oldSelectionBackground = ViewModel.SelectionBackground;

			OptionsWindow optionsWindow = new OptionsWindow() { DataContext = ViewModel, Owner = this };
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

		private void CommandAbout_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			AboutWindow aboutWindow = new AboutWindow() { Owner = this, DataContext = ViewModel };
			aboutWindow.ShowDialog();
		}

		#endregion

		#endregion

	}
}
