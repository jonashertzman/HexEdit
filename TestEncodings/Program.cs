using System.Text;

namespace TestEncodings;

internal class Program
{

	static void Main(string[] args)
	{
		string workingDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;

		string outDirectory = @"..\..\..\..\HexEdit\ManualTests\FileEncodings";

		string p = Path.Combine(outDirectory, "test");

		string s = "ABC def\nÅÄÖ\nДЖЗ\n日本語𱍊";

		// UTF-8
		File.WriteAllText(Path.Combine(outDirectory, "UTF8.txt"), s, new UTF8Encoding(false));
		File.WriteAllText(Path.Combine(outDirectory, "UTF8-BOM.txt"), s, new UTF8Encoding(true));

		// UTF-16
		File.WriteAllText(Path.Combine(outDirectory, "UTF16-BE.txt"), s, new UnicodeEncoding(true, false));
		File.WriteAllText(Path.Combine(outDirectory, "UTF16-BE-BOM.txt"), s, new UnicodeEncoding(true, true));
		File.WriteAllText(Path.Combine(outDirectory, "UTF16-LE.txt"), s, new UnicodeEncoding(false, false));
		File.WriteAllText(Path.Combine(outDirectory, "UTF16-LE-BOM.txt"), s, new UnicodeEncoding(false, true));

		// UTF-32
		File.WriteAllText(Path.Combine(outDirectory, "UTF32-LE.txt"), s, new UTF32Encoding(false, false));
		File.WriteAllText(Path.Combine(outDirectory, "UTF32-LE-BOM.txt"), s, new UTF32Encoding(false, true));
		File.WriteAllText(Path.Combine(outDirectory, "UTF32-BE.txt"), s, new UTF32Encoding(true, false));
		File.WriteAllText(Path.Combine(outDirectory, "UTF32-BE-BOM.txt"), s, new UTF32Encoding(true, true));
	}

}
