using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace adobe_font_extractor_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static class G
        {
            public static string versionNumber = "v1.0.0";
            public static string ccFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Adobe";
            public static string outputFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Adobe\Fonts";
            public static bool verboseLogging = false;
            public static bool openFolderOnFinish = false;
        }

        public MainWindow()
        {
            InitializeComponent();

            outputFolder.Text = G.outputFolderPath;
            ccFolder.Text = G.ccFolderPath;
            G.verboseLogging = (bool)verboseLog.IsChecked;
            G.openFolderOnFinish = (bool)openFolderOnFinish.IsChecked;
            DataContext = this;

            logBox.UpdateLog("Adobe Font Extractor " + G.versionNumber, Brushes.Black);
        }

        private void OutputFolderSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Please select an output folder.",
                UseDescriptionForTitle = true
            };

            if ((bool)dialog.ShowDialog(this))
            {
                outputFolder.Text = dialog.SelectedPath;
                G.outputFolderPath = dialog.SelectedPath;
            }

            if (G.verboseLogging == true)
            {
                logBox.UpdateLog("Changed output folder to " + dialog.SelectedPath, Brushes.Black);
            }
        }

        private void OutputFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (G.verboseLogging == true && outputFolder.Text != G.outputFolderPath)
            {
                logBox.UpdateLog("Changed output folder to " + outputFolder.Text, Brushes.Black);
            }

            G.outputFolderPath = outputFolder.Text;
        }

        private void CCFolderSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog()
            {
                Description = "Please select the Adobe Creative Cloud data folder.",
                UseDescriptionForTitle= true
            };

            if ((bool)dialog.ShowDialog(this))
            {
                ccFolder.Text = dialog.SelectedPath;
                G.ccFolderPath = dialog.SelectedPath;
            }

            if (G.verboseLogging == true)
            {
                logBox.UpdateLog("Changed CC folder to " + dialog.SelectedPath, Brushes.Black);
            }
        }

        private void CCFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (G.verboseLogging == true && ccFolder.Text != G.ccFolderPath)
            {
                logBox.UpdateLog("Changed CC folder to " + ccFolder.Text, Brushes.Black);
            }

            G.ccFolderPath = ccFolder.Text;
        }

        private void VerboseLog_Click(object sender, RoutedEventArgs e)
        {
            G.verboseLogging = (bool)verboseLog.IsChecked;
        }

        private void OpenFolderOnFinish_Click(object sender, RoutedEventArgs e)
        {
            G.openFolderOnFinish = (bool)openFolderOnFinish.IsChecked;
        }

        public class Font
        {
            public string FontName { get; set; }
            public string FontPath { get; set; }
            public bool IsChecked { get; set; }
        }

        public ObservableCollection<Font> FontItems { get; set; }

        private void RetrieveFonts_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker retrieveWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            retrieveWorker.DoWork += RetrieveWorker_DoWork;
            retrieveWorker.RunWorkerCompleted += RetrieveWorker_RunWorkerCompleted;
            retrieveWorker.RunWorkerAsync();
        }

        private void RetrieveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = RetrieveFontsFunc();

            if (Convert.ToInt32(e.Result) != 0)
            {
                return;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void RetrieveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled != true)
            {
                copyFonts.IsEnabled = true;
                logBox.UpdateLog("Found " + e.Result.ToString() + " fonts.", Brushes.Green);
            }
            else
            {
                return;
            }
            
        }

        private int RetrieveFontsFunc()
        {
            string[] fontList = new string[0];

            try
            {
                fontList = Directory.GetFiles(G.ccFolderPath + @"\CoreSync\plugins\livetype\r");
            }
            catch (DirectoryNotFoundException dirEx)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Font folder not found. " + dirEx, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    logBox.UpdateLog("Font folder not found.", Brushes.Red);
                });
                return 0;
            }

            this.Dispatcher.Invoke(() =>
            {
                listFontBox.IsSelectAllActive = true;
            });
            
            FontItems = new ObservableCollection<Font>();

            foreach (string fontPath in fontList)
            {
                PrivateFontCollection fontCollection = new PrivateFontCollection();

                fontCollection.AddFontFile(fontPath);

                string fontName = fontCollection.Families[0].Name;

                Font font = new Font
                {
                    FontName = fontName,
                    IsChecked = true,
                    FontPath = fontPath
                };

                this.Dispatcher.Invoke(() =>
                {
                    FontItems.Add(font);
                    listFontBox.ItemsSource = FontItems;
                });

                fontCollection.Dispose();

                if (G.verboseLogging == true)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        logBox.UpdateLog("Found " + font.FontName, Brushes.Black);
                    });
                }
            }
            return fontList.Length;
        }

        private void CopyFonts_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker copyWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            copyWorker.DoWork += CopyWorker_DoWork;
            copyWorker.RunWorkerCompleted += CopyWorker_RunWorkerCompleted;
            copyWorker.RunWorkerAsync();
        }

        private void CopyWorker_DoWork (object sender, DoWorkEventArgs e)
        {
            e.Result = CopyFontsFunc();

            if (Convert.ToInt32(e.Result) != 0)
            {
                e.Cancel = true;
            }
            else
            {
                return;
            }
        }

        private void CopyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                return;
            }
            else
            {
                logBox.UpdateLog("Copy completed.", Brushes.Green);
                if (G.openFolderOnFinish)
                {
                    Process.Start(G.outputFolderPath);
                }
            }
        }

        private int CopyFontsFunc()
        {
            int folderExists = 0;
            if (!Directory.Exists(G.outputFolderPath))
            {
                
                MessageBoxResult result = MessageBox.Show("Output folder not found. Create folder?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Error);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Directory.CreateDirectory(G.outputFolderPath);
                        logBox.UpdateLog("Creating folder " + G.outputFolderPath, Brushes.Black);
                        folderExists = 0;
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show("Please select a valid output folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        logBox.UpdateLog("Output folder not found.", Brushes.Red);
                        folderExists = 1;
                        break;
                }
            }

            if (listFontBox.SelectedItems.Count != 0 && folderExists == 0)
            {
                foreach (Font font in listFontBox.SelectedItems)
                {
                    if (font.IsChecked == true)
                    {
                        string newFontPath = System.IO.Path.ChangeExtension(G.outputFolderPath + "\\" + font.FontName, ".otf");

                        File.Copy(font.FontPath, newFontPath, true);
                        File.SetAttributes(newFontPath, FileAttributes.Normal);

                        if (G.verboseLogging == true)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                logBox.UpdateLog("Copied " + font.FontName, Brushes.Black);
                            });
                        }
                    }
                }
                return 0;
            }
            else if (listFontBox.SelectedItems.Count != 0)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Please select some fonts first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    logBox.UpdateLog("Please select some fonts first.", Brushes.Red);
                });
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }
}
