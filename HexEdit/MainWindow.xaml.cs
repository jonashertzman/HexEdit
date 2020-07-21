using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace HexEdit
{
	public partial class MainWindow : Window
	{

		#region Members

		MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();

		readonly byte[] UTF8_BOM = { 0xEF, 0xBB, 0xBF };
		readonly byte[] UTF16LE_BOM = { 0xFF, 0xFE };
		readonly byte[] UTF16BE_BOM = { 0xFE, 0xFF };
		readonly byte[] UTF32LE_BOM = { 0x00, 0x00, 0xFE, 0xFF };
		readonly byte[] UTF32BE_BOM = { 0xFE, 0xFF, 0x00, 0x00 };

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

				// First check if the file has a BOM
				if (bytes.Length > 2 && bytes[0..3].SequenceEqual(UTF8_BOM))
				{
					ViewModel.FilePreview = PreviewMode.Utf8;
					chunks.Add(new Chunk(ChunkType.Bom, 0, bytes[0..3]));
					ParseUtf8(bytes, 3, ref chunks);
				}
				else if (bytes.Length > 1 && bytes[0..2].SequenceEqual(UTF16LE_BOM))
				{
					ViewModel.FilePreview = PreviewMode.Utf16le;
					chunks.Add(new Chunk(ChunkType.Bom, 0, bytes[0..2]));
					ParseUtf16le(bytes, 2, ref chunks);
				}
				else if (bytes.Length > 1 && bytes[0..2].SequenceEqual(UTF16BE_BOM))
				{
					ViewModel.FilePreview = PreviewMode.Utf16be;
					chunks.Add(new Chunk(ChunkType.Bom, 0, bytes[0..2]));
					ParseUtf16be(bytes, 2, ref chunks);
				}
				else if (bytes.Length > 3 && bytes[0..4].SequenceEqual(UTF32LE_BOM))
				{
					ViewModel.FilePreview = PreviewMode.Utf32le;
				}
				else if (bytes.Length > 3 && bytes[0..4].SequenceEqual(UTF32BE_BOM))
				{
					ViewModel.FilePreview = PreviewMode.Utf32be;
				}

				// No bom found, check if data passes as a bom-less UTF-8 file
				else if (ParseUtf8(bytes, 0, ref chunks))
				{
					ViewModel.FilePreview = PreviewMode.Utf8;
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

		private void ParseUtf16le(byte[] bytes, int offset, ref ObservableCollection<Chunk> chunks)
		{
			for (int i = offset; i < bytes.Length; i += 2)
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
		}

		private void ParseUtf16be(byte[] bytes, int offset, ref ObservableCollection<Chunk> chunks)
		{
			for (int i = offset; i < bytes.Length; i += 2)
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
		}

		private bool ParseUtf8(byte[] bytes, int offset, ref ObservableCollection<Chunk> chunks)
		{
			int i = offset;

			bool valid = true;

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
				if (bytes.Length <= end) return false;

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
				if (bytes.Length <= end) return false;

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
				if (bytes.Length <= end) return false;


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

				valid = false;
				i++;
			}
			return valid;
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
