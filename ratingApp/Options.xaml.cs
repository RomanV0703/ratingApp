using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace ratingApp
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;

            checkBoxName.IsChecked = Properties.Settings.Default.ColumnsName;
            checkBoxNameRussian.IsChecked = Properties.Settings.Default.ColumnsNameRussian;
            checkBoxYear.IsChecked = Properties.Settings.Default.ColumnsYear;
            checkBoxRating.IsChecked = Properties.Settings.Default.ColumnsRating;
            checkBoxLink.IsChecked = Properties.Settings.Default.ColumnsLink;
            checkBoxVideoQuality.IsChecked = Properties.Settings.Default.ColumnsVideoQuality;
            checkBoxAudioQuality.IsChecked = Properties.Settings.Default.ColumnsAudioQuality;
            checkBoxPlot.IsChecked = Properties.Settings.Default.ColumnsPlot;
            checkBoxPoster.IsChecked = Properties.Settings.Default.ColumnsPoster;            

            foreach (CheckBox checkBox in grid1.Children)
            {
                checkBox.Checked += new RoutedEventHandler(checkBox_Checked);
                checkBox.Unchecked += new RoutedEventHandler(checkBox_Unhecked);
            }
        }

        private void hideColumns()
        {
            Properties.Settings.Default.ColumnsName = (bool)checkBoxName.IsChecked;
            Properties.Settings.Default.ColumnsNameRussian = (bool)checkBoxNameRussian.IsChecked;
            Properties.Settings.Default.ColumnsYear = (bool)checkBoxYear.IsChecked;
            Properties.Settings.Default.ColumnsRating = (bool)checkBoxRating.IsChecked;
            Properties.Settings.Default.ColumnsLink = (bool)checkBoxLink.IsChecked;
            Properties.Settings.Default.ColumnsVideoQuality = (bool)checkBoxVideoQuality.IsChecked;
            Properties.Settings.Default.ColumnsAudioQuality = (bool)checkBoxAudioQuality.IsChecked;
            Properties.Settings.Default.ColumnsPlot = (bool)checkBoxPlot.IsChecked;
            Properties.Settings.Default.ColumnsPoster = (bool)checkBoxPoster.IsChecked;

            Properties.Settings.Default.Save();

            MainWindow mainWindow = Owner as MainWindow;
            mainWindow.hideColumns();
        }


        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            hideColumns();
        }

        private void checkBox_Unhecked(object sender, RoutedEventArgs e)
        {
            hideColumns();
        }

    }
}
