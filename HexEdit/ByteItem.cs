using System.ComponentModel;

namespace HexEdit
{
	public class ByteItem : INotifyPropertyChanged
	{

		#region Constructor

		public ByteItem(byte v)
		{
			ByteValue = v;
		}

		#endregion

		byte? byteValue;
		public byte? ByteValue
		{
			get { return byteValue; }
			set { byteValue = value; OnPropertyChanged(nameof(ByteValue)); }
		}

		public string HexValue
		{
			get { return byteValue?.ToString("X2"); }
		}


		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion
	}
}
