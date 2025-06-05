namespace HexEdit;

static class Extensions
{

	public static bool StartsWith<T>(this T[] source, T[] compare)
	{
		if (source == null || compare == null)
		{
			return false;
		}

		if (source.Length >= compare.Length)
		{
			if (source[0..compare.Length].SequenceEqual(compare))
			{
				return true;
			}
		}

		return false;
	}

}
