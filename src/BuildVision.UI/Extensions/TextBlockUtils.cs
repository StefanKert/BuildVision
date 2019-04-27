using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Extensions
{
    public class TextBlockUtils
    {
        public static readonly DependencyProperty AutoTooltipProperty = DependencyProperty.RegisterAttached(
            "AutoTooltip",
            typeof(bool),
            typeof(TextBlockUtils),
            new PropertyMetadata(false, OnAutoTooltipPropertyChanged));

        public static bool GetAutoTooltip(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoTooltipProperty);
        }

        public static void SetAutoTooltip(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoTooltipProperty, value);
        }

        private static void OnAutoTooltipPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = obj as TextBlock;
            if (textBlock == null)
            {
                return;
            }

            if (e.NewValue.Equals(true))
            {
                textBlock.TextTrimming = TextTrimming.WordEllipsis;
                ComputeAutoTooltip(textBlock);
                textBlock.SizeChanged += TextBlock_SizeChanged;
            }
            else
            {
                textBlock.SizeChanged -= TextBlock_SizeChanged;
            }
        }

        private static void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var textBlock = (TextBlock)sender;
            ComputeAutoTooltip(textBlock);
        }

        private static void ComputeAutoTooltip(TextBlock textBlock)
        {
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double width = textBlock.DesiredSize.Width;
            string tooltip = (textBlock.ActualWidth < width) ? textBlock.Text : null;
            ToolTipService.SetToolTip(textBlock, tooltip);
        }
    }
}
