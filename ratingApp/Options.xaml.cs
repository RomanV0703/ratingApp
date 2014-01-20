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

        string configPath = @".\config.ini";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;

            int i = 0;
            string line;
            
            var file = new StreamReader(configPath);

            var options = new string[10];

            while ((line = file.ReadLine()) != null)
            {
                options[i] = line;
                i++;
            }

            file.Close();

            int n = 0;

            foreach (var column in mainWindow.dataGrid1.Columns)
            {
                var columnCheckBox = new CheckBox();
                columnCheckBox.Content = column.Header;

                foreach (var item in options)
                {
                    if (item.ToLower().Contains(column.Header.ToString().ToLower()))
                    {
                        if (item.ToLower().Contains("show"))
                        {
                            columnCheckBox.IsChecked = true;
                        }

                        else
                        {
                            columnCheckBox.IsChecked = false;
                        }
                    }
                }

                columnCheckBox.Margin = new Thickness(0, n, 0, 0);
                columnCheckBox.Checked += new RoutedEventHandler(columnCheckBox_Checked);
                columnCheckBox.Unchecked += new RoutedEventHandler(columnCheckBox_Unhecked);

                grid2.Children.Add(columnCheckBox);
                //groupBox1.chil
                n += 20;
                //columnCheckBox.SetBinding(ToggleButton.IsCheckedProperty, "DataItem.IsChecked");
            }
        }

        private void columnCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            var checkBox = e.Source as CheckBox;

            foreach (var column in mainWindow.dataGrid1.Columns)
            {
                if (column.Header == checkBox.Content)
                {
                    var file = File.ReadAllText(configPath);

                    file = file.Replace(column.Header.ToString() + " = hide", column.Header.ToString() + " = show");

                    File.WriteAllText(configPath, file);
                }
            }
        }

        private void columnCheckBox_Unhecked(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Owner as MainWindow;
            var checkBox = e.Source as CheckBox;

            foreach (var column in mainWindow.dataGrid1.Columns)
            {
                if (column.Header == checkBox.Content)
                {
                    var file = File.ReadAllText(configPath);

                    file = file.Replace(column.Header.ToString() + " = show", column.Header.ToString() + " = hide");

                    File.WriteAllText(configPath, file);                    
                }
            }
        }
    }
}
