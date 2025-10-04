using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace HexEdit;


[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (targetType != typeof(bool))
			throw new InvalidOperationException("The target must be a Boolean");

		return !(bool)value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}


public class InverseBooleanToVisibilityConverter : IValueConverter
{
	private readonly BooleanToVisibilityConverter converter = new();

	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var result = converter.Convert(value, targetType, parameter, culture) as Visibility?;
		return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		var result = converter.ConvertBack(value, targetType, parameter, culture) as bool?;
		return result != true;
	}
}


public class StringNullOrEmptyToVisibilityConverter : MarkupExtension, IValueConverter
{

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return this;
	}
}



public class RoutedCommandToInputGestureTextConverter : IValueConverter
{

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is RoutedCommand command)
		{
			InputGestureCollection gestures = command.InputGestures;

			if ((gestures != null) && (gestures.Count > 0))
			{
				foreach (KeyGesture keyGesture in gestures)
				{
					if (keyGesture != null)
					{
						return keyGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
					}
				}
			}
		}

		return Binding.DoNothing;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Binding.DoNothing;
	}

}