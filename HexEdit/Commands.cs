using System.Windows.Input;

namespace HexEdit;

public static class Commands
{

	public static readonly RoutedUICommand New = new RoutedUICommand("New", "New", typeof(Commands),
		new InputGestureCollection()
		{
				new KeyGesture(Key.N, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand Open = new RoutedUICommand("Open", "Open", typeof(Commands),
		new InputGestureCollection()
		{
				new KeyGesture(Key.O, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand Save = new RoutedUICommand("Save", "Save", typeof(Commands),
		new InputGestureCollection()
		{
				new KeyGesture(Key.S, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit", "Exit", typeof(Commands),
		new InputGestureCollection()
		{
				new KeyGesture(Key.F4, ModifierKeys.Alt)
		}
	);

	public static readonly RoutedUICommand About = new RoutedUICommand("About", "About", typeof(Commands));

	public static readonly RoutedUICommand Options = new RoutedUICommand("Options", "Options", typeof(Commands));

	public static readonly RoutedUICommand Find = new RoutedUICommand("Find", "Find", typeof(Commands),
		new InputGestureCollection()
		{
				new KeyGesture(Key.F, ModifierKeys.Control)
		}
	);

	public static readonly RoutedUICommand FindNext = new RoutedUICommand("Find Next", "FindNext", typeof(Commands),
		new InputGestureCollection()
		{
				new KeyGesture(Key.G, ModifierKeys.Control),
				new KeyGesture(Key.F3)
		}
	);

	public static readonly RoutedUICommand FindPrevious = new RoutedUICommand("Find Previous", "FindPrevious", typeof(Commands),
		new InputGestureCollection()
		{
				new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift)
		}
	);

	public static readonly RoutedUICommand CloseFind = new RoutedUICommand("Close Find", "CloseFind", typeof(Commands),
		new InputGestureCollection()
		{
				new KeyGesture(Key.Escape)
		}
	);

}
