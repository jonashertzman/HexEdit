namespace HexEdit;

public class ChunkEventArgs : EventArgs
{
	public Chunk SelectedItem { get; }

	public ChunkEventArgs(Chunk fileItem)
	{
		SelectedItem = fileItem;
	}
}
