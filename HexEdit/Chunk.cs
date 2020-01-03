using System.ComponentModel;

namespace HexEdit
{

	public class Chunk : INotifyPropertyChanged
	{

		ChunkType type = ChunkType.None;
		public ChunkType Type
		{
			get { return type; }
			set { type = value; OnPropertyChanged(nameof(Type)); }
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

	}
}