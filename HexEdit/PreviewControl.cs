using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace HexEdit;

public class PreviewControl : Control
{

	#region Members

	private double dpiScale = 0;
	private Typeface typeface;

	private double offsetMargin;
	private double textMargin;

	double lineHeight = -1;
	double byteWidth = -1;

	private readonly Stopwatch stopwatch = new();

	Chunk selectedChunk = null;

	#endregion

	#region Constructor

	static PreviewControl()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(PreviewControl), new FrameworkPropertyMetadata(typeof(PreviewControl)));
	}

	public PreviewControl()
	{
		this.ClipToBounds = true;
	}

	#endregion

	#region Overrides

	protected override void OnRender(DrawingContext drawingContext)
	{
		Debug.Print("PreviewControl OnRender");

#if DEBUG
		MeasureRenderTime();
#endif

		// Fill background
		drawingContext.DrawRectangle(AppSettings.TextBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

		if (Bytes.Count == 0)
			return;

		Stopwatch sw = new();
		sw.Start();

		typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

		Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
		dpiScale = 1 / m.M11;

		double maxTextWidth = 0;
		int bytesPerRow = AppSettings.BytesPerRow;



		int lineCount = Bytes.Count / bytesPerRow + 1;

		TextUtils.CreateGlyphRun("W", typeface, this.FontSize, dpiScale, out double characterWidth);
		double characterHeight = Math.Ceiling(TextUtils.FontHeight(typeface, this.FontSize, dpiScale) / dpiScale) * dpiScale;

		VisibleLines = (int)(ActualHeight / characterHeight + 1);
		MaxVerticalScroll = Bytes.Count / bytesPerRow - VisibleLines + 2;

		int maxOffset = Bytes.Count.ToString("X2").Length;
		double rowOffsetWidth = maxOffset * characterWidth;


		Pen borderPen = new(SystemColors.ScrollBarBrush, RoundToWholePixels(1));
		borderPen.Freeze();
		GuidelineSet borderGuide = CreateGuidelineSet(borderPen);

		Pen chunkPen = new(new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)), RoundToWholePixels(4));
		Pen chunkPen2 = new(new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)), RoundToWholePixels(4));
		chunkPen.Freeze();
		GuidelineSet chunkGuide = CreateGuidelineSet(chunkPen);

		lineHeight = characterHeight + 2 * chunkPen.Thickness;

		textMargin = RoundToWholePixels(4);
		offsetMargin = RoundToWholePixels(rowOffsetWidth) + (2 * textMargin);

		double hexWidth = 0;

		for (int i = 0; i < 16; i++)
		{
			string s = i.ToString("X");
			TextUtils.CreateGlyphRun(s, typeface, FontSize, dpiScale, out double b);
			hexWidth = Math.Max(hexWidth, b);
		}

		byteWidth = RoundToWholePixels(hexWidth) * 2 + chunkPen.Thickness * 4;

		// Draw offset background
		drawingContext.DrawRectangle(SystemColors.ControlBrush, null, new Rect(-0, 0, offsetMargin, this.ActualHeight));

		for (int i = 0; i < VisibleLines; i++)
		{
			int rowByteOffset = (i + VerticalOffset) * bytesPerRow;

			if (rowByteOffset >= Bytes.Count)
				break;

			string previewString = "";

			// Line Y offset
			drawingContext.PushTransform(new TranslateTransform(0, lineHeight * i));
			{
				// Draw row offset
				drawingContext.PushTransform(new TranslateTransform(textMargin, chunkPen.Thickness));
				{
					drawingContext.DrawGlyphRun(SystemColors.ControlDarkBrush, TextUtils.CreateGlyphRun(rowByteOffset.ToString("X2").PadLeft(maxOffset, '0'), typeface, FontSize, dpiScale, out double w));
				}
				drawingContext.Pop();

				drawingContext.PushClip(new RectangleGeometry(new Rect(offsetMargin, 0, byteWidth * bytesPerRow, lineHeight)));
				{
					drawingContext.PushTransform(new TranslateTransform(offsetMargin, 0));
					{

						// Draw bytes
						for (int j = 0; j < bytesPerRow && rowByteOffset + j < Bytes.Count; j++)
						{
							drawingContext.PushTransform(new TranslateTransform(j * byteWidth, 0));
							{
								if ((j + i) % 2 == 0)
									drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, byteWidth, lineHeight));

								drawingContext.PushTransform(new TranslateTransform(chunkPen.Thickness * 2, chunkPen.Thickness));
								{
									drawingContext.DrawGlyphRun(AppSettings.TextForeground, TextUtils.CreateGlyphRun(Bytes[rowByteOffset + j].ToString("X2"), typeface, this.FontSize, dpiScale, out _));
								}
								drawingContext.Pop();
							}
							drawingContext.Pop();
						}

						// Draw chunks
						foreach (Chunk c in Chunks)
						{
							if (c.Type == ChunkType.None)
								continue;

							Pen pen = c == selectedChunk ? chunkPen2 : chunkPen;

							if (!(c.End < rowByteOffset || c.Start > rowByteOffset + bytesPerRow - 1))
							{
								drawingContext.PushClip(new RectangleGeometry(new Rect((c.Start - rowByteOffset) * byteWidth, 0, byteWidth * c.Length, lineHeight)));
								{
									drawingContext.DrawRectangle(null, pen, new Rect((c.Start - rowByteOffset) * byteWidth, 0, c.Length * byteWidth, lineHeight));
								}
								drawingContext.Pop();

								if (c.Start >= rowByteOffset)
								{
									previewString += c.PreviewString;
								}
							}
						}
					}
					drawingContext.Pop();
				}
				drawingContext.Pop(); // Byte area clip

				// Draw preview
				drawingContext.PushTransform(new TranslateTransform(bytesPerRow * byteWidth + 20 + rowOffsetWidth, 0));
				{
					drawingContext.DrawGlyphRun(AppSettings.TextForeground, TextUtils.CreateGlyphRun(previewString, typeface, this.FontSize, dpiScale, out _));
				}
				drawingContext.Pop();
			}
			drawingContext.Pop(); // Line Y offset
		}

		// Draw row offset border
		drawingContext.PushGuidelineSet(borderGuide);
		{
			double borderThickness = RoundToWholePixels(borderPen.Thickness / 2);

			if (borderThickness == 0)
				borderThickness = 1;

			drawingContext.DrawLine(borderPen, new Point(offsetMargin - borderThickness, -1), new Point(offsetMargin - borderThickness, this.ActualHeight));
		}
		drawingContext.Pop();

		TextAreaWidth = (int)ActualWidth;
		MaxHorizontalScroll = (int)(maxTextWidth - TextAreaWidth);


#if DEBUG
		ReportRenderTime();
#endif
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		bool controlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

		if (e.Key == Key.Down)
		{
			VerticalOffset++;
		}
		else if (e.Key == Key.Up)
		{
			VerticalOffset--;
		}
		else if (e.Key == Key.PageUp)
		{
			VerticalOffset = Math.Max(0, VerticalOffset -= VisibleLines - 1);
		}
		else if (e.Key == Key.PageDown)
		{
			VerticalOffset = VerticalOffset += VisibleLines - 1;
		}
		else if (e.Key == Key.Home && controlPressed)
		{
			VerticalOffset = 0;
		}
		else if (e.Key == Key.End && controlPressed)
		{
			VerticalOffset = Bytes.Count / AppSettings.BytesPerRow + 1;
		}
		else
		{
			base.OnKeyDown(e);
			return;
		}

		e.Handled = true;
		InvalidateVisual();
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		Point currentMousePosition = e.GetPosition(this);

		selectedChunk = PointToChunk(currentMousePosition);

		InvalidateVisual();
	}

	private Chunk PointToChunk(Point currentMousePosition)
	{
		int line = (int)(currentMousePosition.Y / lineHeight) + VerticalOffset;
		int column = (int)((currentMousePosition.X - offsetMargin + HorizontalOffset) / byteWidth);

		if (Bytes.Count / AppSettings.BytesPerRow < line) // Below the last line
			return null;

		if (column >= AppSettings.BytesPerRow || line * AppSettings.BytesPerRow + column >= Bytes.Count) // Beyonnd the roghtmost column
			return null;


		int byteIndex = line * AppSettings.BytesPerRow + column;

		Debug.Print($"Line {line} Column {column}  index {byteIndex}");

		foreach (Chunk c in Chunks)
		{
			if (byteIndex >= c.Start && byteIndex <= c.End)
			{
				return c;
			}
		}

		return null;
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


	public static readonly DependencyProperty MaxVerticalScrollProperty = DependencyProperty.Register("MaxVerticalScroll", typeof(int), typeof(PreviewControl));

	public int MaxVerticalScroll
	{
		get { return (int)GetValue(MaxVerticalScrollProperty); }
		set { SetValue(MaxVerticalScrollProperty, value); }
	}


	public static readonly DependencyProperty MaxHorizontalScrollProperty = DependencyProperty.Register("MaxHorizontalScroll", typeof(int), typeof(PreviewControl));

	public int MaxHorizontalScroll
	{
		get { return (int)GetValue(MaxHorizontalScrollProperty); }
		set { SetValue(MaxHorizontalScrollProperty, value); }
	}


	public static readonly DependencyProperty TextAreaWidthProperty = DependencyProperty.Register("TextAreaWidth", typeof(int), typeof(PreviewControl));

	public int TextAreaWidth
	{
		get { return (int)GetValue(TextAreaWidthProperty); }
		set { SetValue(TextAreaWidthProperty, value); }
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

	private void MeasureRenderTime()
	{
		stopwatch.Restart();
	}

	private void ReportRenderTime()
	{
		Dispatcher.BeginInvoke(
			DispatcherPriority.Loaded,
			new Action(() =>
			{
				stopwatch.Stop();
				Debug.Print($"Took {stopwatch.ElapsedMilliseconds} ms");
			})
		);
	}

	private Size MeasureString(string text)
	{
		var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, Brushes.Black, new NumberSubstitution(), TextFormattingMode.Display, dpiScale);

		return new Size(formattedText.Width, formattedText.Height);
	}

	private double RoundToWholePixels(double x)
	{
		return Math.Round(x / dpiScale) * dpiScale;
	}

	private GuidelineSet CreateGuidelineSet(Pen pen)
	{
		GuidelineSet guidelineSet = new();
		guidelineSet.GuidelinesX.Add(pen.Thickness / 2);
		guidelineSet.GuidelinesY.Add(pen.Thickness / 2);
		guidelineSet.Freeze();

		return guidelineSet;
	}

	#endregion

}
