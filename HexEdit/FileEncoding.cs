﻿namespace HexEdit;

internal static class FileEncoding
{

	#region  Members

	static readonly byte[] UTF8_BOM = [0xEF, 0xBB, 0xBF];
	static readonly byte[] UTF32LE_BOM = [0xFF, 0xFE, 0x00, 0x00];
	static readonly byte[] UTF32BE_BOM = [0x00, 0x00, 0xFE, 0xFF];
	static readonly byte[] UTF16LE_BOM = [0xFF, 0xFE];
	static readonly byte[] UTF16BE_BOM = [0xFE, 0xFF];

	#endregion

	#region Methods

	public static Encoding DetectEncoding(byte[] bytes)
	{
		// Check if the file has a BOM
		if (bytes.StartsWith(UTF8_BOM))
		{
			return Encoding.Utf8;
		}
		else if (bytes.StartsWith(UTF32LE_BOM)) // Must check this before UTF16 since the first 2 bytes are the same as an UTF16 little endian BOM.
		{
			return Encoding.Utf32le;
		}
		else if (bytes.StartsWith(UTF32BE_BOM))
		{
			return Encoding.Utf32be;
		}
		else if (bytes.StartsWith(UTF16LE_BOM))
		{
			return Encoding.Utf16le;
		}
		else if (bytes.StartsWith(UTF16BE_BOM))
		{
			return Encoding.Utf16be;
		}

		// No bom found, check if data passes as a bom-less file.
		else if (ValidUtf8(bytes))
		{
			return Encoding.Utf8;
		}
		else if (ValidUtf16le(bytes))
		{
			return Encoding.Utf16le;
		}
		else if (ValidUtf16be(bytes))
		{
			return Encoding.Utf16be;
		}
		else if (ValidUtf32le(bytes))
		{
			return Encoding.Utf32le;
		}
		else if (ValidUtf32be(bytes))
		{
			return Encoding.Utf32be;
		}

		return Encoding.Unknown;
	}

	public static bool ValidUnicodeCharacter(int codePoint)
	{
		return codePoint >= 0x0000_0000 && codePoint <= 0x0010_FFFF && !(codePoint >= 0xD800 && codePoint <= 0xDFFF);
	}

	private static bool ValidUtf8(byte[] bytes)
	{
		int i = 0;

		if (bytes.StartsWith(UTF8_BOM))
		{
			i += UTF8_BOM.Length;
		}

		while (i < bytes.Length)
		{
			// 1 byte character
			if (bytes[i] >= 0x00 && bytes[i] <= 0x7F)
			{
				i++;
				continue;
			}

			// 2 byte character
			if (i + 1 < bytes.Length)
			{
				if (bytes[i] >= 0xC2 && bytes[i] <= 0xDF)
				{
					if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
					{
						i += 2;
						continue;
					}
				}
			}

			// 3 byte character
			if (i + 2 < bytes.Length)
			{
				if (bytes[i] == 0xE0)
				{
					if (bytes[i + 1] >= 0xA0 && bytes[i + 1] <= 0xBF)
					{
						if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
						{
							i += 3;
							continue;
						}
					}
				}

				if (bytes[i] >= 0xE1 && bytes[i] <= 0xEC)
				{
					if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
					{
						if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
						{
							i += 3;
							continue;
						}
					}
				}

				if (bytes[i] == 0xED)
				{
					if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x9F)
					{
						if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
						{
							i += 3;
							continue;
						}
					}
				}

				if (bytes[i] >= 0xEE && bytes[i] <= 0xEF)
				{
					if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
					{
						if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
						{
							i += 3;
							continue;
						}
					}
				}
			}

			// 4 byte character
			if (i + 3 < bytes.Length)
			{
				if (bytes[i] == 0xF0)
				{
					if (bytes[i + 1] >= 0x90 && bytes[i + 1] <= 0xBF)
					{
						if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
						{
							if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
							{
								i += 4;
								continue;
							}
						}
					}
				}

				if (bytes[i] >= 0xF1 && bytes[i] <= 0xF3)
				{
					if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
					{
						if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
						{
							if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
							{
								i += 4;
								continue;
							}
						}
					}
				}

				if (bytes[i] == 0xF4)
				{
					if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x8F)
					{
						if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
						{
							if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
							{
								i += 4;
								continue;
							}
						}
					}
				}
			}

			return false;
		}

		return true;
	}

	private static bool ValidUtf16le(byte[] bytes)
	{
		return false;
	}

	private static bool ValidUtf16be(byte[] bytes)
	{
		return false;
	}

	private static bool ValidUtf32le(byte[] bytes)
	{
		return false;
	}

	private static bool ValidUtf32be(byte[] bytes)
	{
		return false;
	}


	public static List<Chunk> ParseDataAs(Encoding foundEncoding, byte[] bytes)
	{
		return foundEncoding switch
		{
			Encoding.Utf8 => ParseUtf8(bytes),
			Encoding.Utf16le => ParseUtf16le(bytes),
			Encoding.Utf16be => ParseUtf16be(bytes),
			Encoding.Utf32le => ParseUtf32le(bytes),
			Encoding.Utf32be => ParseUtf32be(bytes),
			_ => ParseDefault(bytes),
		};
	}

	private static List<Chunk> ParseDefault(byte[] bytes)
	{
		return [];
	}

	private static List<Chunk> ParseUtf8(byte[] bytes)
	{
		List<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF8_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF8_BOM));
			i += UTF8_BOM.Length;
		}

		while (i < bytes.Length)
		{
			// 1 byte character
			int end = i + 1;
			if (bytes[i] > 0x00 && bytes[i] <= 0x7F)
			{
				chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
				i++;
				continue;
			}

			// 2 byte character
			end = i + 2;
			if (bytes.Length < end) break;

			if (bytes[i] >= 0xC2 && bytes[i] <= 0xDF)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
					i += 2;
					continue;
				}
			}

			// 3 byte character
			end = i + 3;
			if (bytes.Length < end) break;

			if (bytes[i] == 0xE0)
			{
				if (bytes[i + 1] >= 0xA0 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] >= 0xE1 && bytes[i] <= 0xEC)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] == 0xED)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x9F)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
						i += 3;
						continue;
					}
				}
			}

			if (bytes[i] >= 0xEE && bytes[i] <= 0xEF)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
						i += 3;
						continue;
					}
				}
			}

			// 4 byte character
			end = i + 4;
			if (bytes.Length < end) break;

			if (bytes[i] == 0xF0)
			{
				if (bytes[i + 1] >= 0x90 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
							i += 4;
							continue;
						}
					}
				}
			}

			if (bytes[i] >= 0xF1 && bytes[i] <= 0xF3)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
							i += 4;
							continue;
						}
					}
				}
			}

			if (bytes[i] == 0xF4)
			{
				if (bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x8F)
				{
					if (bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)
					{
						if (bytes[i + 3] >= 0x80 && bytes[i + 3] <= 0xBF)
						{
							chunks.Add(new Chunk(ChunkType.Utf8Character, i, bytes[i..end]));
							i += 4;
							continue;
						}
					}
				}
			}

			i++;
		}

		return chunks;
	}

	private static List<Chunk> ParseUtf16le(byte[] bytes)
	{
		List<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF16LE_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF16LE_BOM));
			i += UTF16LE_BOM.Length;
		}

		for (; i < bytes.Length - 1; i += 2)
		{
			if (char.IsHighSurrogate((char)(bytes[i + 1] << 8 | bytes[i])))
			{
				chunks.Add(new Chunk(ChunkType.Utf16leCharacter, i, bytes[i..(i + 4)]));
				i += 2;
			}
			else
			{
				chunks.Add(new Chunk(ChunkType.Utf16leCharacter, i, bytes[i..(i + 2)]));
			}
		}

		return chunks;
	}

	private static List<Chunk> ParseUtf16be(byte[] bytes)
	{
		List<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF16BE_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF16BE_BOM));
			i += UTF16BE_BOM.Length;
		}

		for (; i < bytes.Length - 1; i += 2)
		{
			if (char.IsHighSurrogate((char)(bytes[i] << 8 | bytes[i + 1])))
			{
				Chunk c = new(ChunkType.Utf16beCharacter, i, bytes[i..(i + 4)]);
				if (c.IsValidCharacter)
				{
					chunks.Add(c);
				}
				i += 2;
			}
			else
			{
				Chunk c = new(ChunkType.Utf16beCharacter, i, bytes[i..(i + 2)]);
				if (c.IsValidCharacter)
				{
					chunks.Add(c);
				}
			}
		}

		return chunks;
	}

	private static List<Chunk> ParseUtf32le(byte[] bytes)
	{
		List<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF32LE_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF32LE_BOM));
			i += UTF32LE_BOM.Length;
		}

		for (; i < bytes.Length - 3; i += 4)
		{
			chunks.Add(new Chunk(ChunkType.Utf32leCharacter, i, bytes[i..(i + 4)]));
		}

		return chunks;
	}

	private static List<Chunk> ParseUtf32be(byte[] bytes)
	{
		List<Chunk> chunks = [];

		int i = 0;

		if (bytes.StartsWith(UTF32BE_BOM))
		{
			chunks.Add(new Chunk(ChunkType.Bom, i, UTF32BE_BOM));
			i += UTF32BE_BOM.Length;
		}

		for (; i < bytes.Length - 3; i += 4)
		{
			chunks.Add(new Chunk(ChunkType.Utf32beCharacter, i, bytes[i..(i + 4)]));
		}

		return chunks;
	}

	#endregion

}
