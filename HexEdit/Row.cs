using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HexEdit
{
	public class Row : INotifyPropertyChanged
	{


		ObservableCollection<ByteItem> bytes = new ObservableCollection<ByteItem>();
		public ObservableCollection<ByteItem> Bytes
		{
			get { return bytes; }
			set { bytes = value; OnPropertyChanged(nameof(Bytes)); }
		}

		int offset;
		public int Offset
		{
			get { return offset; }
			set { offset = value; OnPropertyChanged(nameof(Offset)); }
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
