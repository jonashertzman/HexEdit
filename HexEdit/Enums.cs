using System.ComponentModel;

namespace HexEdit;

public enum ChunkType
{
	None,
	Bom,
	Utf8Character,
	Utf16leCharacter,
	Utf16beCharacter,
	Utf32leCharacter,
	Utf32beCharacter
}

public enum Encoding
{
	Ascii,
	Ansi,
	[Description("UTF 8")]
	Utf8,
	Utf16be,
	Utf16le,
	Utf32be,
	Utf32le,
	Unknown,
}
