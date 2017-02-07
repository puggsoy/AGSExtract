using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using Ookii.Dialogs.Wpf;

namespace AGSExtractWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AGSGame game;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.DefaultExt = "*.*";
            d.Filter = "All Files (*.*)|*.*";

            if(d.ShowDialog() == true)
            {
                openFile(d.FileName, d.SafeFileName);
            }
        }

        private void openFile(string path, string name)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open);

                if (game != null) game.close();

                game = AGSGame.load(fs);

                foreach (string n in game.fNames)
                {
                    checkListBox.Items.Add(n);
                    checkListBox.SelectedItems.Add(checkListBox.Items[checkListBox.Items.Count - 1]);
                }

                MessageBox.Show("Successfully opened " + name + ".", "File open successful", MessageBoxButton.OK);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "File open failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void extractBtn_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog d = new VistaFolderBrowserDialog();

            if(d.ShowDialog() == true)
            {
                extractFiles(d.SelectedPath);
            }
        }

        private void extractFiles(string outDir)
        {
            try
            {
                foreach(string s in checkListBox.SelectedItems)
                {
                    byte[] file = game.getFile(s);
                    FileStream fs = new FileStream(System.IO.Path.Combine(outDir, s), FileMode.Create);
                    fs.Write(file, 0, file.Length);
                    fs.Close();
                }

                MessageBox.Show("Successfully extracted selected files!", "Extraction successful", MessageBoxButton.OK);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Exaction failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void onItemCheck(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            if (checkListBox.SelectedItems.Count == 0)
                extractBtn.IsEnabled = false;
            else
                extractBtn.IsEnabled = true;
        }

        private void checkAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach(string s in checkListBox.Items)
                checkListBox.SelectedItems.Add(s);
        }

        private void uncheckAllBtn_Click(object sender, RoutedEventArgs e)
        {
            while (checkListBox.SelectedItems.Count > 0)
                checkListBox.SelectedItems.RemoveAt(0);
        }
    }
}
