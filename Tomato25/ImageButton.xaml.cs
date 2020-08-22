using System.Windows;
using System.Windows.Media;

namespace Tomato25 {
    public partial class ImageButton {
        public ImageButton() {
            InitializeComponent();
        }

        public ImageSource Image {
            get => (ImageSource)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty
            .Register("Image", typeof(ImageSource), typeof(ImageButton),
                    new UIPropertyMetadata(null));
    }
}