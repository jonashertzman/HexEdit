using System.Windows.Input;

namespace HexEdit;

public static class Commands
{

	public static readonly RoutedUICommand New = new("New", "New", typeof(Commands),
		[new KeyGesture(Key.N, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand Open = new("Open", "Open", typeof(Commands),
		[new KeyGesture(Key.O, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand Save = new("Save", "Save", typeof(Commands),
		[new KeyGesture(Key.S, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand Exit = new("Exit", "Exit", typeof(Commands),
		[new KeyGesture(Key.F4, ModifierKeys.Alt)]
	);

	public static readonly RoutedUICommand About = new("About", "About", typeof(Commands));

	public static readonly RoutedUICommand Options = new("Options", "Options", typeof(Commands));

	public static readonly RoutedUICommand Find = new("Find", "Find", typeof(Commands),
		[new KeyGesture(Key.F, ModifierKeys.Control)]
	);

	public static readonly RoutedUICommand FindNext = new("Find Next", "FindNext", typeof(Commands),
		[new KeyGesture(Key.G, ModifierKeys.Control), new KeyGesture(Key.F3)]
	);

	public static readonly RoutedUICommand FindPrevious = new("Find Previous", "FindPrevious", typeof(Commands),
		[new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift)]
	);

	public static readonly RoutedUICommand CloseFind = new("Close Find", "CloseFind", typeof(Commands),
		[new KeyGesture(Key.Escape)]
	);

}
