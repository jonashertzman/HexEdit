using System.Text;

namespace TestEncodings;

internal class Program
{

	static void Main(string[] args)
	{
		string fileEncodingsDirectory = @"..\..\..\..\HexEdit\ManualTests\FileEncodings";

		string exampleText = "ABC def\nÅÄÖ\nДЖЗ\n日本語𱍊\nA\u030A\n";

		// UTF-8
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF8.txt"), exampleText, new UTF8Encoding(false));
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF8-BOM.txt"), exampleText, new UTF8Encoding(true));

		// UTF-16
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF16-BE.txt"), exampleText, new UnicodeEncoding(true, false));
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF16-BE-BOM.txt"), exampleText, new UnicodeEncoding(true, true));
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF16-LE.txt"), exampleText, new UnicodeEncoding(false, false));
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF16-LE-BOM.txt"), exampleText, new UnicodeEncoding(false, true));

		// UTF-32
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF32-LE.txt"), exampleText, new UTF32Encoding(false, false));
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF32-LE-BOM.txt"), exampleText, new UTF32Encoding(false, true));
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF32-BE.txt"), exampleText, new UTF32Encoding(true, false));
		File.WriteAllText(Path.Combine(fileEncodingsDirectory, "UTF32-BE-BOM.txt"), exampleText, new UTF32Encoding(true, true));


		string testsDirectory = @"..\..\..\..\HexEdit\ManualTests";

		// Characters that can be represented as a single code point, but also as a combination of multiple code points
		string combinedText = "Å\u0041\u030A\nå\u0061\u030A";
		File.WriteAllText(Path.Combine(testsDirectory, "CombinedCharacters.txt"), combinedText, new UnicodeEncoding(true, false));

		// Characters that are represented as surrogate pairs in UTF-16
		string surrogateText = "abc\n𐐷𤭢";
		File.WriteAllText(Path.Combine(testsDirectory, "SurrogateCharacters.txt"), surrogateText, new UnicodeEncoding(true, false));
	}

}
