using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
//-----------------------------------------------------------------------------
namespace Pomodoro {
	public class BoolNullConverter : MarkupExtension, IValueConverter {
		public static BoolNullConverter Instance = new BoolNullConverter();
		//---------------------------------------------------------------------
		BoolNullConverter() { }
		//---------------------------------------------------------------------
		public override object ProvideValue(IServiceProvider serviceProvider) {
			return this;
		}
		//---------------------------------------------------------------------
		public object Convert(object value, Type targetType,
				object parameter, CultureInfo culture) {
			return value != null;
		}
		//---------------------------------------------------------------------
		public object ConvertBack(object value, Type targetType,
				object parameter, CultureInfo culture) {
			return (bool) value ? new object() : null;
		}
	}
}