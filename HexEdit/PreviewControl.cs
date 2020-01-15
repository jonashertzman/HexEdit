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
			drawingContext.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			if (Bytes.Count == 0)
				return;

			typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

			Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			dpiScale = 1 / m.M11;

			double characterHeight = MeasureString("A").Height;
			double maxTextwidth = 0;
			int rowLength = 8;


			VisibleLines = (int)(ActualHeight / characterHeight + 1);
			MaxVerialcalScroll = Bytes.Count / rowLength - VisibleLines + 2;

			for (int i = 0; i < VisibleLines; i++)
			{
				int rowOffset = (i + VerticalOffset) * rowLength;

				// Line Y offset
				drawingContext.PushTransform(new TranslateTransform(0, characterHeight * i));
				{
					for (int J = 0; J < rowLength && rowOffset + J < Bytes.Count; J++)
					{
						drawingContext.DrawText(new FormattedText(Bytes[rowOffset + J].ToString("X2"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, Brushes.Black, new NumberSubstitution(), TextFormattingMode.Display, dpiScale), new Point(J * 30, 0));
					}
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
