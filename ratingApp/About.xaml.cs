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
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        public Window3()
        {
            InitializeComponent();
        }

        private void About_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Text = "Last Updated: " + File.GetLastWriteTime(@".\IMDB Rating.exe").ToString("dd.MM.yyyy");
            textBlock1.Text = "IMDB Rating \nv.0.5";
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
