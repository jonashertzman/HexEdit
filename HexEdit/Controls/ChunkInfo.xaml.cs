using System.Windows;
using System.Windows.Controls;

namespace HexEdit.Controls
{

	public partial class ChunkInfo : UserControl
	{

		public ChunkInfo()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty SelectedChunkProperty = DependencyProperty.Register(
			nameof(SelectedChunk), typeof(Chunk), typeof(ChunkInfo), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public Chunk SelectedChunk
		{
			get { return (Chunk)GetValue(SelectedChunkProperty); }
			set
			{
				Debug.WriteLine("dsfsadf");

				SetValue(SelectedChunkProperty, value);
			}
		}

	}

}
