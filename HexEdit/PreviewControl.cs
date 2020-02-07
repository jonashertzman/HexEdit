using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HexEdit
{
	public class PreviewControl : Control
	{

		#region Members

		private double dpiScale = 0;
		private Typeface typeface;

		#endregion

		#region Constructor

		static PreviewControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PreviewControl), new FrameworkPropertyMetadata(typeof(PreviewControl)));
		}

		#endregion

		#region Overrides

		protected override void OnRender(DrawingContext drawingContext)
		{
			Debug.Print("PreviewControl OnRender");

			// Fill background
			drawingContext.DrawRectangle(AppSettings.TextBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			if (Bytes.Count == 0)
				return;

			typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

			Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			dpiScale = 1 / m.M11;

			double characterHeight = MeasureString("A").Height;
			double maxTextwidth = 0;
			int bytesPerRow = AppSettings.BytesPerRow;
			int byteWidth = 20;

			Pen chunkPen = new Pen(AppSettings.TextForeground, 1);

			VisibleLines = (int)(ActualHeight / characterHeight + 1);
			MaxVerialcalScroll = Bytes.Count / bytesPerRow - VisibleLines + 2;

			for (int i = 0; i < VisibleLines; i++)
			{
				int rowByteOffset = (i + VerticalOffset) * bytesPerRow;
				string previewString = "";

				// Line Y offset
				drawingContext.PushTransform(new TranslateTransform(0, characterHeight * i));
				{
					// Byte area clip
					drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, byteWidth * bytesPerRow + 5, characterHeight + 1)));
					{
						foreach (Chunk c in Chunks)
						{
							if (!(c.End < rowByteOffset || c.Start > rowByteOffset + bytesPerRow - 1))
							{
								drawingContext.PushTransform(new TranslateTransform(-.5, .5));
								{
									drawingContext.DrawRectangle(new SolidColorBrush(Colors.LightGray), chunkPen, new Rect((c.Start - rowByteOffset) * byteWidth + 4, 0, c.Length * byteWidth - 3, characterHeight));
								}
								drawingContext.Pop();
								if (c.Start >= rowByteOffset)
								{
									previewString += c.PreviewString;
								}
							}
						}
					}
					drawingContext.Pop(); // Byte area clip

					for (int j = 0; j < bytesPerRow && rowByteOffset + j < Bytes.Count; j++)
					{
						drawingContext.DrawText(new FormattedText(Bytes[rowByteOffset + j].ToString("X2"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, AppSettings.TextForeground, new NumberSubstitution(), TextFormattingMode.Display, dpiScale), new Point(j * byteWidth + 5, 0));
					}

					drawingContext.DrawText(new FormattedText(previewString, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, AppSettings.TextForeground, new NumberSubstitution(), TextFormattingMode.Display, dpiScale), new Point(bytesPerRow * byteWidth + 20, 0));
				}
				drawingContext.Pop(); // Line Y offset
			}

			TextAreaWidth = (int)ActualWidth;
			MaxHorizontalScroll = (int)(maxTextwidth - TextAreaWidth);
		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty BytesProperty = DependencyProperty.Register("Bytes", typeof(ObservableCollection<byte>), typeof(PreviewControl), new FrameworkPropertyMetadata(new ObservableCollection<byte>(), FrameworkPropertyMetadataOptions.AffectsRender));

		public ObservableCollection<byte> Bytes
		{
			get { return (ObservableCollection<byte>)GetValue(BytesProperty); }
			set { SetValue(BytesProperty, value); }
		}


		public static readonly DependencyProperty ChunksProperty = DependencyProperty.Register("Chunks", typeof(ObservableCollection<Chunk>), typeof(PreviewControl), new FrameworkPropertyMetadata(new ObservableCollection<Chunk>(), FrameworkPropertyMetadataOptions.AffectsRender));

		public ObservableCollection<Chunk> Chunks
		{
			get { return (ObservableCollection<Chunk>)GetValue(ChunksProperty); }
			set { SetValue(ChunksProperty, value); }
		}


		public static readonly DependencyProperty VisibleLinesProperty = DependencyProperty.Register("VisibleLines", typeof(int), typeof(PreviewControl));

		public int VisibleLines
		{
			get { return (int)GetValue(VisibleLinesProperty); }
			set { SetValue(VisibleLinesProperty, value); }
		}


		public static readonly DependencyProperty MaxVerialcalScrollProperty = DependencyProperty.Register("MaxVerialcalScroll", typeof(int), typeof(PreviewControl));

		public int MaxVerialcalScroll
		{
			get { return (int)GetValue(MaxVerialcalScrollProperty); }
			set { SetValue(MaxVerialcalScrollProperty, value); }
		}


		public static readonly DependencyProperty MaxHorizontalScrollPropery = DependencyProperty.Register("MaxHorizontalScroll", typeof(int), typeof(PreviewControl));

		public int MaxHorizontalScroll
		{
			get { return (int)GetValue(MaxHorizontalScrollPropery); }
			set { SetValue(MaxHorizontalScrollPropery, value); }
		}


		public static readonly DependencyProperty TextAreaWidthPropery = DependencyProperty.Register("TextAreaWidth", typeof(int), typeof(PreviewControl));

		public int TextAreaWidth
		{
			get { return (int)GetValue(TextAreaWidthPropery); }
			set { SetValue(TextAreaWidthPropery, value); }
		}


		public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(int), typeof(PreviewControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int VerticalOffset
		{
			get { return (int)GetValue(VerticalOffsetProperty); }
			set { SetValue(VerticalOffsetProperty, value); }
		}


		public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(int), typeof(PreviewControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int HorizontalOffset
		{
			get { return (int)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}


		public static readonly DependencyProperty UpdateTriggerProperty = DependencyProperty.Register("UpdateTrigger", typeof(int), typeof(PreviewControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int UpdateTrigger
		{
			get { return (int)GetValue(UpdateTriggerProperty); }
			set { SetValue(UpdateTriggerProperty, value); }
		}

		#endregion

		#region Methods

		private Size MeasureString(string text)
		{
			var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, Brushes.Black, new NumberSubstitution(), TextFormattingMode.Display, dpiScale);

			return new Size(formattedText.Width, formattedText.Height);
		}

		#endregion

	}
}
