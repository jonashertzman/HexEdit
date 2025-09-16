using System.ComponentModel;

namespace HexEdit;

public class Chunk : INotifyPropertyChanged
{

	#region Constructor

	public Chunk(ChunkType type, int start, byte[] bytes)
	{
		Type = type;
		Start = start;
		Length = bytes.Length;

		switch (type)
		{
			case ChunkType.Utf8Character:
				UnicodeCharacter = DecodeUtf8(bytes);
				break;

			case ChunkType.Utf16leCharacter:
				UnicodeCharacter = DecodeUtf16le(bytes);
				break;

			case ChunkType.Utf16beCharacter:
				UnicodeCharacter = DecodeUtf16be(bytes);
				break;

			case ChunkType.Utf32leCharacter:
				UnicodeCharacter = DecodeUtf32le(bytes);
				break;

			case ChunkType.Utf32beCharacter:
				UnicodeCharacter = DecodeUtf32be(bytes);
				break;

		}
	}

	#endregion

	#region Overrides

	public override string ToString()
	{
		return $"{Start} - {Length}   {Type}";
	}

	#endregion

	#region Properties

	public ChunkType Type { get; internal set; } = ChunkType.None;

	public int Start { get; internal set; }

	public int UnicodeCharacter
	{
		get
		{
			return field;
		}
		internal set
		{
			if (value >= 0x0000_0000 && value <= 0x0010_FFFF )
			{
				if (value >= 0xD800 && value <= 0xDFFF) // Reserved range for surrogate code points
				{
					value = -1;
				}

				field = value;
			}
		}
	} = -1;

	public int Length { get; private set; }

	public int End
	{
		get
		{
			return Start + Length - 1;
		}
	}

	public string PreviewString
	{
		get
		{
			switch (Type)
			{
				case ChunkType.Bom:
					return "[BOM]";

				case ChunkType.Utf8Character:
				case ChunkType.Utf16leCharacter:
				case ChunkType.Utf16beCharacter:
				case ChunkType.Utf32leCharacter:
				case ChunkType.Utf32beCharacter:
					if (UnicodeCharacter != -1)
						return char.ConvertFromUtf32(UnicodeCharacter);
					return "[UNKNOWN]";


				default:
					return "[UNKNOWN]";

			}
		}
	}

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

	private int DecodeUtf16le(byte[] bytes)
	{
		if (bytes.Length == 2)
		{
			return bytes[1] << 8 | bytes[0];
		}
		else // 4 byte surrogate pair character
		{
			int highSurrogate = bytes[1] << 8 | bytes[0];
			int lowSurrogate = bytes[3] << 8 | bytes[2];

			highSurrogate -= 0xD800;
			highSurrogate *= 0x400;
			lowSurrogate -= 0xDC00;

			return highSurrogate + lowSurrogate + 0x10000;
		}
	}

	private int DecodeUtf16be(byte[] bytes)
	{
		if (bytes.Length == 2)
		{
			return bytes[0] << 8 | bytes[1];
		}
		else // 4 byte surrogate pair character
		{
			int highSurrogate = bytes[0] << 8 | bytes[1];
			int lowSurrogate = bytes[2] << 8 | bytes[3];

			highSurrogate -= 0xD800;
			highSurrogate *= 0x400;
			lowSurrogate -= 0xDC00;

			return highSurrogate + lowSurrogate + 0x10000;
		}
	}

	private int DecodeUtf32le(byte[] bytes)
	{
		int i = BitConverter.ToInt32(bytes, 0);

		return i;
	}

	private int DecodeUtf32be(byte[] bytes)
	{
		int i = bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];

		return i;
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
