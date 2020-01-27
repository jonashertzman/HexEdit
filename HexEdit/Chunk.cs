using System.ComponentModel;

namespace HexEdit
{

	public class Chunk : INotifyPropertyChanged
	{

		#region Constructor

		public Chunk(ChunkType type, int start, byte[] bytes)
		{
			this.Type = type;
			this.Start = start;

			this.bytes = bytes;

			switch (type)
			{
				case ChunkType.Utf8Character:
					UnicodeCharacter = DecodeUtf8(bytes);
					break;
			}
		}

		#endregion

		#region Properties

		public ChunkType Type { get; internal set; } = ChunkType.None;

		public int Start { get; internal set; }

		public int UnicodeCharacter { get; internal set; } = -1;

		public int Length
		{
			get
			{
				return bytes.Length;
			}
		}

		byte[] bytes = new byte[0];

		#endregion

		#region Methods

		private int DecodeUtf8(byte[] bytes)
		{
			int character = 0;
			if (bytes.Length == 1)
			{
				character = bytes[0] & 0b0111_1111;
			}
			else if (bytes.Length == 2)
			{
				character = bytes[0] & 0b0001_1111;
			}
			else if (bytes.Length == 3)
			{
				character = bytes[0] & 0b0000_1111;
			}
			else if (bytes.Length == 4)
			{
				character = bytes[0] & 0b0000_0111;
			}

			int i = 0;
			while (++i < bytes.Length)
			{
				character <<= 6;
				character |= bytes[i] & 0b0011_1111;
			}

			return character;
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
}