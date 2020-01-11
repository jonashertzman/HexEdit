using System.ComponentModel;

namespace HexEdit
{

	public enum ChunkType
	{
		None,
		Bom,
		Utf8,
		Utf16,
		Utf32
	}

	public enum PreviewMode
	{
		ASCII,
		Ansi,
		[Description("UTF 8")]
		UTF8,
		UTF16BE,
		UTF16LE,
		UTF32BE,
		UTF32LE
	}

}
