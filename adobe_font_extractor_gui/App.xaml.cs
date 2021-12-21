using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace adobe_font_extractor_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
    }

    public static class Extensions
    {
        public static void UpdateLog(this RichTextBox box, string text, System.Windows.Media.Brush color)
        {
            TextRange time = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd)
            {
                Text = DateTime.Now.ToString("HH:mm:ss") + "\t"
            };
            time.ApplyPropertyValue(TextElement.ForegroundProperty, System.Windows.Media.Brushes.LightGray);

            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd)
            {
                Text = text
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);

            box.AppendText(Environment.NewLine);
            box.ScrollToEnd();
        }
    }
}
