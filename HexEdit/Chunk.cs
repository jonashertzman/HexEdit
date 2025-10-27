using System.ComponentModel;

namespace HexEdit;

public class Chunk : INotifyPropertyChanged
{

	#region Constructor

	public Chunk(ChunkType type, int start, byte[] bytes, int codePoint = -1)
	{
		Type = type;
		Start = start;
		Length = bytes.Length;

		if (CharacterType(type))
		{
			if (FileEncoding.ValidUnicodeCharacter(codePoint))
			{
				UnicodeCharacter = codePoint;
			}
			else
			{
				Type = ChunkType.None;
			}
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
		get;
		internal set
		{
			Debug.Assert(FileEncoding.ValidUnicodeCharacter(value));

			field = value;
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

	private bool CharacterType(ChunkType type)
	{
		return type.In([
			ChunkType.Utf8Character,
			ChunkType.Utf16beCharacter,
			ChunkType.Utf16leCharacter,
			ChunkType.Utf32beCharacter,
			ChunkType.Utf32leCharacter,
		]);
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
