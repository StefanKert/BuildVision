using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Extensions
{
    public class TextBlockUtils
    {
        /// <summary>
        /// Identified the attached AutoTooltip property.
        /// When <c>true</c>, this will set the <see cref="TextBlock.TextTrimming"/>
        /// property to <see cref="TextTrimming.WordEllipsis"/>, and display a tooltip
        /// with the full text whenever the text is trimmed.
        /// </summary>
        public static readonly DependencyProperty AutoTooltipProperty = DependencyProperty.RegisterAttached(
            "AutoTooltip",
            typeof(bool),
            typeof(TextBlockUtils),
            new PropertyMetadata(false, OnAutoTooltipPropertyChanged));

        /// <summary>
        /// Gets the value of the <see cref="AutoTooltipProperty"/> dependency property.
        /// </summary>
        public static bool GetAutoTooltip(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoTooltipProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="AutoTooltipProperty"/> dependency property.
        /// </summary>
        public static void SetAutoTooltip(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoTooltipProperty, value);
        }

        private static void OnAutoTooltipPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = obj as TextBlock;
            if (textBlock == null)
                return;

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

        /// <summary>
        /// Assigns the ToolTip for the given TextBlock based on whether the text is trimmed.
        /// </summary>
        private static void ComputeAutoTooltip(TextBlock textBlock)
        {
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double width = textBlock.DesiredSize.Width;
            string tooltip = (textBlock.ActualWidth < width) ? textBlock.Text : null;
            ToolTipService.SetToolTip(textBlock, tooltip);
        }
    }
}
