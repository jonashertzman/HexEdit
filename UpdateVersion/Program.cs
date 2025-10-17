using System.IO.Compression;

namespace UpdateVersion;

class Program
{
	static void Main()
	{
		DateTime buildDate = DateTime.Now;
		string buildNumber = $"{buildDate:yy}{buildDate.DayOfYear:D3}";

		Console.WriteLine($"Updating version to {buildNumber}");

		File.WriteAllText(@"..\docs\download\version.txt", buildNumber);


		Console.WriteLine($"Updating download");

		File.Delete(@"..\docs\download\HexEdit.zip");

		using ZipArchive download = ZipFile.Open(@"..\docs\download\HexEdit.zip", ZipArchiveMode.Create);
		download.CreateEntryFromFile(@".\bin\Publish\HexEdit.exe", "HexEdit.exe");
		download.CreateEntryFromFile(@"..\LICENSE", "LICENSE");
	}
}
