using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace RCL.Win.Resources
{
    // Code-behind for Styles.xaml (ResourceDictionary)
    public partial class Styles : ResourceDictionary
    {
        public Styles()
        {
            // This will be called by the XAML build-generated InitializeComponent in WPF build.
            // No runtime initialization required here.
        }
    }

    /// <summary>
    /// Lightweight watermark (placeholder) helper for TextBox.
    /// Usage in XAML:
    ///   xmlns:wm="clr-namespace:RCL.Win.Resources"
    ///   &lt;TextBox Style="{StaticResource TextBoxStyle}" wm:WatermarkService.Watermark="Search..." /&gt;
    /// 
    /// The Watermark is implemented with an Adorner so it does not modify text.
    /// </summary>
    public static class WatermarkService
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached(
                "Watermark",
                typeof(string),
                typeof(WatermarkService),
                new PropertyMetadata(null, OnWatermarkChanged));

        public static void SetWatermark(DependencyObject element, string value) => element.SetValue(WatermarkProperty, value);
        public static string GetWatermark(DependencyObject element) => (string)element.GetValue(WatermarkProperty);

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb)
            {
                tb.Loaded -= Tb_Loaded;
                tb.Loaded += Tb_Loaded;
                tb.TextChanged -= Tb_TextChanged;
                tb.TextChanged += Tb_TextChanged;
                tb.Unloaded -= Tb_Unloaded;
                tb.Unloaded += Tb_Unloaded;

                // if already loaded, ensure adorner state
                if (tb.IsLoaded) UpdateAdorner(tb);
            }
        }

        private static void Tb_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                RemoveAdorner(tb);
            }
        }

        private static void Tb_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                UpdateAdorner(tb);
            }
        }

        private static void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                UpdateAdorner(tb);
            }
        }

        private static void UpdateAdorner(TextBox tb)
        {
            var text = tb.Text;
            var watermark = GetWatermark(tb);
            if (string.IsNullOrEmpty(watermark)) { RemoveAdorner(tb); return; }

            if (string.IsNullOrEmpty(text))
            {
                AddAdorner(tb, watermark);
            }
            else
            {
                RemoveAdorner(tb);
            }
        }

        private static void AddAdorner(TextBox tb, string watermark)
        {
            var layer = AdornerLayer.GetAdornerLayer(tb);
            if (layer == null) return;

            var adorners = layer.GetAdorners(tb);
            if (adorners != null)
            {
                foreach (var a in adorners)
                {
                    if (a is WatermarkAdorner) return; // already exists
                }
            }

            var wa = new WatermarkAdorner(tb, watermark);
            layer.Add(wa);
        }

        private static void RemoveAdorner(TextBox tb)
        {
            var layer = AdornerLayer.GetAdornerLayer(tb);
            if (layer == null) return;

            var adorners = layer.GetAdorners(tb);
            if (adorners == null) return;

            foreach (var a in adorners)
            {
                if (a is WatermarkAdorner wa)
                {
                    layer.Remove(wa);
                }
            }
        }

        private class WatermarkAdorner : Adorner
        {
            private readonly TextBlock _textBlock;

            public WatermarkAdorner(UIElement adornedElement, string watermark) : base(adornedElement)
            {
                IsHitTestVisible = false;
                _textBlock = new TextBlock
                {
                    Text = watermark,
                    FontStyle = FontStyles.Normal,
                    Opacity = 0.55,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0, 0, 0),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                // match the adorned TextBox font/size if possible
                if (adornedElement is Control c)
                {
                    _textBlock.FontFamily = c.FontFamily;
                    _textBlock.FontSize = c.FontSize;
                }
            }

            protected override int VisualChildrenCount => 1;
            protected override Visual GetVisualChild(int index) => _textBlock;

            protected override Size MeasureOverride(Size constraint)
            {
                _textBlock.Measure(constraint);
                return _textBlock.DesiredSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _textBlock.Arrange(new Rect(new Point(2, 0), finalSize));
                return finalSize;
            }
        }
    }
}

