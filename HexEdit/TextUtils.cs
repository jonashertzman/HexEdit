﻿using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace HexEdit;

static class TextUtils
{

	#region Members

	static readonly Typeface defaultTypeface = new("Courier New");

	static FontData fontCache;

	static Typeface cachedTypeface;
	static double cachedFontSize;

	#endregion

	#region Constructor

	static TextUtils()
	{
	}

	#endregion

	#region Methods

	static FontData GetFontData(Typeface typeface, double fontSize)
	{
		if (!typeface.Equals(cachedTypeface) || fontSize != cachedFontSize)
		{
			if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
			{
				defaultTypeface.TryGetGlyphTypeface(out glyphTypeface);
			}
			bool charactersFound = GetTextBounds(glyphTypeface, fontSize, out double topDistance, out double bottomDistance);

			fontCache = new FontData(glyphTypeface, topDistance, bottomDistance, charactersFound);

			cachedTypeface = typeface;
			cachedFontSize = fontSize;
		}

		return fontCache;
	}

	internal static GlyphRun CreateGlyphRun(string text, Typeface typeface, double fontSize, double dpiScale, out double runWidth)
	{
		if (text.Length == 0)
		{
			runWidth = 0;
			return null;
		}

		FontData fontData = GetFontData(typeface, fontSize);

		ushort[] glyphIndexes = new ushort[text.Length];
		double[] advanceWidths = new double[text.Length];

		double totalWidth = 0;
		int codePoint;
		for (int n = 0; n < text.Length; n++)
		{
			// C# uses UTF16 encoded strings which for some characters requires 2 surrogate pair chars to encode 
			// one character, if so we add a zero width space glyph after the real glyph to keep the number of 
			// glyphs in the glyph run the same as the number of chars in the string.
			if (char.IsHighSurrogate(text[n]))
			{
				codePoint = char.ConvertToUtf32(text, n);
			}
			else if (char.IsLowSurrogate(text[n]))
			{
				codePoint = '\u200B';
			}
			else
			{
				codePoint = text[n];
			}

			ushort glyphIndex = ReplaceGlyph(codePoint, fontData.GlyphTypeface, fontSize, dpiScale, out double width);

			glyphIndexes[n] = glyphIndex;
			advanceWidths[n] = width;

			totalWidth += width;
		}

		double maxTopDistance = fontData.TopDistance;

		if (fontData.GlyphTypeface.Baseline * fontSize > maxTopDistance || !fontData.HeightsCalculated)
		{
			maxTopDistance = fontData.GlyphTypeface.Baseline * fontSize * fontData.GlyphTypeface.Height;
		}

		maxTopDistance = Math.Ceiling(maxTopDistance / dpiScale) * dpiScale;

		GlyphRun run = new(
			glyphTypeface: fontData.GlyphTypeface,
			bidiLevel: 0,
			isSideways: false,
			renderingEmSize: Math.Ceiling(fontSize / dpiScale) * dpiScale,
			pixelsPerDip: (float)dpiScale,
			glyphIndices: glyphIndexes,
			baselineOrigin: new Point(0, maxTopDistance),
			advanceWidths: advanceWidths,
			glyphOffsets: null,
			characters: null,
			deviceFontName: null,
			clusterMap: null,
			caretStops: null,
			language: null);

		runWidth = totalWidth;
		return run;
	}

	private static ushort ReplaceGlyph(int codePoint, GlyphTypeface glyphTypeface, double fontSize, double dpiScale, out double width)
	{
		glyphTypeface.CharacterToGlyphMap.TryGetValue('W', out ushort wIndex);
		double characterWidth = Math.Ceiling(glyphTypeface.AdvanceWidths[wIndex] * fontSize / dpiScale) * dpiScale;

		int displayCodePoint = codePoint;

		glyphTypeface.CharacterToGlyphMap.TryGetValue(displayCodePoint, out ushort glyphIndex);
		width = Math.Ceiling(glyphTypeface.AdvanceWidths[glyphIndex] * fontSize / dpiScale) * dpiScale;
		return glyphIndex;
	}

	public static double FontHeight(Typeface typeface, double fontSize, double dpiScale)
	{
		FontData fontData = GetFontData(typeface, fontSize);

		if (!fontData.HeightsCalculated)
		{
			return MeasureText("A", typeface, fontSize, dpiScale).Height;
		}

		double topDistance = fontData.TopDistance;

		if (fontData.GlyphTypeface.Baseline * fontSize > fontData.TopDistance)
		{
			topDistance = fontData.GlyphTypeface.Baseline * fontSize * fontData.GlyphTypeface.Height;
		}

		return (Math.Ceiling(topDistance / dpiScale) * dpiScale) + (Math.Ceiling(fontData.BottomDistance / dpiScale) * dpiScale);
	}

	private static bool GetTextBounds(GlyphTypeface glyphTypeface, double fontSize, out double topDistance, out double bottomDistance)
	{
		string testCharacters = "aA. ÅÄÖÃÂ_[]{}()|ygf";

		topDistance = double.MaxValue;
		bottomDistance = double.MinValue;

		for (int n = 0; n < testCharacters.Length; n++)
		{
			if (glyphTypeface.CharacterToGlyphMap.TryGetValue(testCharacters[n], out ushort glyphIndex))
			{
				Geometry outline = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, 0);
				topDistance = Math.Min(topDistance, outline.Bounds.Top);
				bottomDistance = Math.Max(bottomDistance, outline.Bounds.Bottom);
			}
		}

		if (bottomDistance == double.MinValue)
		{
			return false;
		}

		topDistance = Math.Abs(topDistance);
		return true;
	}

	private static Size MeasureText(string text, Typeface typeface, double fontSize, double dpiScale)
	{
		FormattedText formattedText = new(
			text,
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			typeface,
			fontSize,
			Brushes.Black,
			new NumberSubstitution(),
			TextFormattingMode.Display,
			dpiScale);

		return new Size(formattedText.Width, formattedText.Height);
	}

	private static string FindFont(int codePoint)
	{
		foreach (FontFamily family in Fonts.SystemFontFamilies)
		{
			foreach (Typeface typeface in family.GetTypefaces())
			{
				typeface.TryGetGlyphTypeface(out GlyphTypeface glyph);
				if (glyph != null && glyph.CharacterToGlyphMap.TryGetValue(codePoint, out _))
				{
					if (family.FamilyNames.TryGetValue(XmlLanguage.GetLanguage("en-us"), out string familyName))
					{
						return familyName;
					}
				}
			}
		}
		return "";
	}

	#endregion

}
