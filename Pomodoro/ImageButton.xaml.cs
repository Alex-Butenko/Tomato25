using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
//-----------------------------------------------------------------------------
namespace Pomodoro {
	public partial class ImageButton : Button {
		public ImageButton() {
			InitializeComponent();
		}
		//---------------------------------------------------------------------
		public ImageSource Image {
			get {
				return (ImageSource) GetValue(ImageProperty);
			}
			set {
				SetValue(ImageProperty, value);
			}
		}
		//---------------------------------------------------------------------
		public static readonly DependencyProperty ImageProperty = DependencyProperty
			.Register("Image", typeof(ImageSource), typeof(ImageButton),
					new UIPropertyMetadata(null));
		//---------------------------------------------------------------------
		public ImageSource MouseOverImage {
			get {
				return (ImageSource) GetValue(MouseOverImageProperty);
			}
			set {
				SetValue(MouseOverImageProperty, value);
			}
		}
		//---------------------------------------------------------------------
		public static readonly DependencyProperty MouseOverImageProperty
			= DependencyProperty.Register("MouseOverImage", typeof(ImageSource),
					typeof(ImageButton), new UIPropertyMetadata(null));
		//---------------------------------------------------------------------
		public ImageSource PressedImage {
			get {
				return (ImageSource) GetValue(PressedImageProperty);
			}
			set {
				SetValue(PressedImageProperty, value);
			}
		}
		//---------------------------------------------------------------------
		public static readonly DependencyProperty PressedImageProperty
			= DependencyProperty.Register("PressedImage", typeof(ImageSource),
					typeof(ImageButton), new UIPropertyMetadata(null));
		//---------------------------------------------------------------------
		public ImageSource DisabledImage {
			get {
				return (ImageSource) GetValue(DisabledImageProperty);
			}
			set {
				SetValue(DisabledImageProperty, value);
			}
		}
		//---------------------------------------------------------------------
		public static readonly DependencyProperty DisabledImageProperty
			= DependencyProperty.Register("DisabledImage", typeof(ImageSource),
					typeof(ImageButton), new UIPropertyMetadata(null));
	}
}