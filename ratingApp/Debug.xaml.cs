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

namespace ratingApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Debug : Window
    {
        public Debug()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        void Refresh()
        {
            MainWindow mainWindow = Owner as MainWindow;

            int count = 0;

            foreach (var item in mainWindow.raw_list)
            {
                textBox1.Text += item.ToString() + "\n";
                count += 1;
            }

            label3.Content = count.ToString();

            dataGrid1.ItemsSource = mainWindow.final_list.OrderBy(Movie => Movie.Name).ToList();
                        
            dataGrid1.Columns.Single(c => c.Header.ToString() == "Poster").Visibility = System.Windows.Visibility.Hidden;
            dataGrid1.Columns.Single(c => c.Header.ToString() == "Plot").Visibility = System.Windows.Visibility.Hidden;
            dataGrid1.Columns.Single(c => c.Header.ToString() == "Rating").Visibility = System.Windows.Visibility.Hidden;

            label6.Content = mainWindow.final_list.Count;

            //dataGrid2.ItemsSource = mainWindow.debugList;            

            //label4.Content = mainWindow.debugList.Count;
        }

        private void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
