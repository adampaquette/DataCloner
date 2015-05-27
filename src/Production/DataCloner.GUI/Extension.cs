using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DataCloner.GUI
{
    internal static class Extension
    {
        public static void AppendTextWithColor(this RichTextBox rtb, string text, SolidColorBrush BackgroundBrush)
        {
            var p = new Paragraph();
            p.Margin = new Thickness(0);
            p.Padding = new Thickness(0);

            var span = new Span();
            span.Background = Brushes.LightCyan;
            span.Inlines.Add(new Run(text));

            p.Inlines.Add(span);
            rtb.Document.Blocks.Add(p);
        }
    }
}
