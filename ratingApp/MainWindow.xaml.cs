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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.Web;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;
using System.Net;
using static System.Windows.SystemParameters;

namespace ratingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            listBox_videoQuality.ItemsSource = videoQuality_list;

            listBox_audioQuality.ItemsSource = audioQuality_list;

            Closed += new EventHandler(MainWindow_Closed);                                 
        }

        #region ReferencesFromResources
        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return System.Reflection.Assembly.Load(bytes);
        }
        #endregion        

        void MainWindow_Closed(object sender, System.EventArgs e)
        {
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        public class Movie
        {
            public string Name { get; set; }
            public string NameRussian { get; set; }
            public string Year { get; set; }
            public string Rating { get; set; }
            public string Link { get; set; }
            public string videoQuality { get; set; }
            public string audioQuality { get; set; }
            public string Plot { get; set; }
            public string Poster { get; set; }
        }

        public class RutrackerLink
        {
            public string ID { get; set; }
            public string Link { get; set; }
        }

        public class DebugAudioVideo
        {
            public string Type { get; set; }
            public string Content { get; set; }
        }

        List<Movie> buffer = new List<Movie>();
        List<Movie> filterResult = new List<Movie>();

        List<Movie> rating_list = new List<Movie>();
        public List<string> raw_list = new List<string>();
        List<string> links_list = new List<string>();
        public List<string> parsed_list = new List<string>();
        public List<Movie> final_list = new List<Movie>();
        List<string> videoQuality_list = new List<string>(new string[] {
                                                                    "HDRip-AVC",
                                                                    "HDRip",                                                                    
                                                                    "BDRip-AVC",
                                                                    "BDRip",                                                                    
                                                                    "DVDRip",
                                                                    "DVDScr",
                                                                    "DVD",
                                                                    "CAMRip",
                                                                    "TeleSync",
                                                                    "SATRip",
                                                                    "VHSRip",                                                                    
                                                                    "DVB",   
                                                                    "HDTVRip-AVC",
                                                                    "HDTVRip",
                                                                    "HDTV",
                                                                    "WEB-DLRip",
                                                                    "WEB-DL",
                                                                    "WEB-Rip",
                                                                    "TVRip",
                                                                  });
        List<string> audioQuality_list = new List<string>(new string[] { 
                                                                    "AVO",                                                       
                                                                    "DVO",
                                                                    "MVO",
                                                                    "VO",
                                                                    "Dub",
                                                                    "Sub",
                                                                    "Original"
                                                                  });
        List<string> years_list = new List<string>();

        private void pagesHandling()
        {                                     
            progressBar1.Value = 0;            

            ParseRutracker(comboBox1.SelectedValue.ToString() + "&start=" + page.ToString());

            listBox_years.SelectAll();
            listBox_videoQuality.SelectAll();
            listBox_audioQuality.SelectAll();

            applyFilters();

            if (filterResult.Count == 0)
            {
                next_btn.IsEnabled = false;
                //textBox1.Text = (int.Parse(textBox1.Text) - 1).ToString();
            }
            else
            {
                next_btn.IsEnabled = true;
            }

            if (page == 0)
            {
                previous_btn.IsEnabled = false;
            }
            else
            {
                previous_btn.IsEnabled = true;
            }
        }

        private void applyFilters()
        {
            #region Select/Deselect buttons
            if (listBox_years.SelectedItems.Count == listBox_years.Items.Count)
            {
                button_selectAllYears.Content = "Deselect all";
            }
            else
            {
                button_selectAllYears.Content = "Select all";
            }

            if (listBox_videoQuality.SelectedItems.Count == listBox_videoQuality.Items.Count)
            {
                button_selectAllVideo.Content = "Deselect all";
            }
            else
            {
                button_selectAllVideo.Content = "Select all";
            }

            if (listBox_audioQuality.SelectedItems.Count == listBox_audioQuality.Items.Count)
            {
                button_selectAllAudio.Content = "Deselect all";
            }
            else
            {
                button_selectAllAudio.Content = "Select all";
            } 
            #endregion

            buffer.Clear();
            filterResult.Clear();

            filterResult.AddRange(final_list);

            if (textBox_name.Text.Trim() != "")
            {
                buffer.AddRange(filterResult.FindAll(listItem => listItem.Name.ToLower().Contains(textBox_name.Text.ToLower())));
            }
            else
            {
                buffer.AddRange(filterResult);
            }

            filterResult.Clear();
            filterResult.AddRange(buffer);
            buffer.Clear();

            if (listBox_videoQuality.SelectedItems.Count != 0 || listBox_audioQuality.SelectedItems.Count !=0 || listBox_years.SelectedItems.Count != 0)
            {
                foreach (var videoQualityItem in listBox_videoQuality.SelectedItems)
                {
                    buffer.AddRange(filterResult.FindAll(listItem => listItem.videoQuality == videoQualityItem.ToString()));
                }

                filterResult.Clear();
                filterResult.AddRange(buffer);
                buffer.Clear();

                foreach (var audioQualityItem in listBox_audioQuality.SelectedItems)
                {
                    buffer.AddRange(filterResult.FindAll(listItem => listItem.audioQuality == audioQualityItem.ToString()));
                }

                filterResult.Clear();
                filterResult.AddRange(buffer);
                buffer.Clear();

                foreach (var yearsItem in listBox_years.SelectedItems)
                {
                    buffer.AddRange(filterResult.FindAll(listItem => listItem.Year == yearsItem.ToString()));
                }

                filterResult.Clear();
                filterResult.AddRange(buffer);
                buffer.Clear();
            }

            filterResult = filterResult.OrderBy(Movie => Movie.Name).ToList();

            if (rating_list.Count != 0)
            {
                filterResult = filterResult.OrderByDescending(Movie => Movie.Rating).ToList();
            }

            dataGrid1.ItemsSource = filterResult;

            label5.Content = filterResult.Count.ToString();

            hideColumns();
        }

        public void hideColumns()
        {
            if (!dataGrid1.Items.IsEmpty)
            {
                if (Properties.Settings.Default.ColumnsName == false)
                    dataGrid1.Columns[0].Visibility = Visibility.Hidden;
                else
                    dataGrid1.Columns[0].Visibility = Visibility.Visible;

                if (Properties.Settings.Default.ColumnsNameRussian == false)
                    dataGrid1.Columns[1].Visibility = Visibility.Hidden;
                else
                    dataGrid1.Columns[1].Visibility = Visibility.Visible;

                if (Properties.Settings.Default.ColumnsYear == false)
                    dataGrid1.Columns[2].Visibility = Visibility.Hidden;
                else
                    dataGrid1.Columns[2].Visibility = Visibility.Visible;

                if (Properties.Settings.Default.ColumnsRating == false)
                    dataGrid1.Columns[3].Visibility = Visibility.Hidden;
                else
                    dataGrid1.Columns[3].Visibility = Visibility.Visible;

                if (Properties.Settings.Default.ColumnsLink == false)
                    dataGrid1.Columns[4].Visibility = Visibility.Hidden;
                else
                    dataGrid1.Columns[4].Visibility = Visibility.Visible;

                if (Properties.Settings.Default.ColumnsVideoQuality == false)
                    dataGrid1.Columns[5].Visibility = Visibility.Hidden;
                else
                    dataGrid1.Columns[5].Visibility = Visibility.Visible;

                if (Properties.Settings.Default.ColumnsAudioQuality == false)
                    dataGrid1.Columns[6].Visibility = Visibility.Hidden;
                else
                    dataGrid1.Columns[6].Visibility = Visibility.Visible;

                if (Properties.Settings.Default.ColumnsPlot == false)
                    dataGrid1.Columns[7].Visibility = Visibility.Hidden;
                else
                    dataGrid1.Columns[7].Visibility = Visibility.Visible;

                if (Properties.Settings.Default.ColumnsPoster == false)
                    dataGrid1.Columns[8].Visibility = Visibility.Hidden;
                else
                    dataGrid1.Columns[8].Visibility = Visibility.Visible;

            }
            //if (!dataGrid1.Items.IsEmpty)
            //{
            //    int i = 0;
            //    string line;

            //    // Read the file and display it line by line.
            //    var file = new System.IO.StreamReader(@".\config.ini");

            //    var options = new string[11];

            //    while ((line = file.ReadLine()) != null)
            //    {
            //        options[i] = line;
            //        i++;
            //    }

            //    file.Close();

            //    foreach (var column in this.dataGrid1.Columns)
            //    {
            //        var columnCheckBox = new CheckBox();
            //        columnCheckBox.Content = column.Header;

            //        foreach (var item in options)
            //        {
            //            if (item.ToLower().Contains(column.Header.ToString().ToLower()))
            //            {
            //                if (item.ToLower().Contains("show"))
            //                {
            //                    column.Visibility = System.Windows.Visibility.Visible;
            //                }
            //                else
            //                {
            //                    column.Visibility = System.Windows.Visibility.Hidden;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public void openWebLink(string webLink)
        {
            Process myProcess = new Process();

            try
            {
                // true is the default, but it is important not to set it to false
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = webLink;
                myProcess.Start();
            }

            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public string removeAfter(List<string> list, string input, string character)
        {
            int index = input.IndexOf(character);
            if (index > 0)
            {
                input = input.Substring(0, index);
                return input;
            }
            else
            {
                return input;
            }
        }

        public root deserializeFromXMLDoc(XmlDocument toDeserialize)
        {
            root movie;

            var serializer = new XmlSerializer(typeof(root));
            movie = (root)serializer.Deserialize(new XmlNodeReader(toDeserialize.DocumentElement));

            return movie;
        }

        public void GetIMBDRating(BackgroundWorker worker)
        {
            rating_list.Clear();

            System.Net.WebClient wc = new System.Net.WebClient();

            int max = filterResult.Count;
            double one = 100 / max;
            int sum = 0;

            foreach (Movie movie in filterResult)
            {
                worker.ReportProgress(sum);
                sum += Convert.ToInt32(one);

                string name = movie.Name.Trim();
                //name = HttpUtility.HtmlEncode(name);                
                name = name.Replace(' ', '+');
                name = name.Replace(":", "%3A");
                name = name.Replace("'", "%27");
                string query = "http://www.omdbapi.com/?t=" + name + "&y=" + movie.Year + "&r=xml";
                string webData = wc.DownloadString(query);

                string pattern = "<a href(.*?)>";

                var matches = Regex.Matches(webData, pattern); // Check for href-links in movies' plot texts

                if (matches.Count != 0)
                {
                    foreach (Match m in matches)
                    {
                        webData = webData.Replace(m.ToString(), "");
                    }
                }

                var xmlMovie = new XmlDocument();
                xmlMovie.LoadXml(webData);

                var deserializedMovie = deserializeFromXMLDoc(xmlMovie);

                string rating = string.Empty;
                string plot = string.Empty;
                string poster = string.Empty;

                if (deserializedMovie.response == "True")
                {
                    var currentMovie = deserializedMovie.movie[0];

                    rating = currentMovie.imdbRating.ToString();
                    plot = currentMovie.plot;
                    poster = currentMovie.poster;
                    poster = Regex.Replace(poster, @"SX\d{3,}", "SX100"); // Get smaller poster to fit popup
                }
                else
                {
                    rating = "N/A";
                    plot = "N/A";
                    poster = "N/A";
                }

                rating_list.Add(new Movie
                {
                    Name = movie.Name,
                    Year = movie.Year,
                    Rating = rating,
                    Link = movie.Link,
                    videoQuality = movie.videoQuality,
                    audioQuality = movie.audioQuality,
                    NameRussian = movie.NameRussian,
                    Plot = plot,
                    Poster = poster
                });
            }

            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                    {
                        final_list = rating_list;
                        applyFilters();
                    }));

            }

            catch (Exception exceptionObject)
            {
                MessageBox.Show(exceptionObject.ToString());
            }
        }

        public void ClearAll()
        {
            dataGrid1.ItemsSource = null;

            raw_list.Clear();
            parsed_list.Clear();
            final_list.Clear();
            links_list.Clear();
        }

        public void RunInBackground()
        {
        }

        public void ParseRutracker(string url)
        {
            ClearAll();

            var webGet = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.GetEncoding("windows-1251")
            };

            HtmlDocument document = null;
            try
            {
                document = webGet.Load(url);
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            HtmlNodeCollection nodes = null;

            if (document != null)
            {
                nodes = document.DocumentNode.SelectNodes("//td[@class='tt']/a");
            }

            if (nodes == null)
            {                
                return;
            }

            foreach (HtmlNode link in nodes)
            {
                string parsedLinkText = link.InnerText.ToString();
                string parsedLinkAddress = link.Attributes["href"].Value.ToString();
                int dotIndex = parsedLinkAddress.IndexOf('=');
                parsedLinkAddress = parsedLinkAddress.Substring(dotIndex + 1);
                parsedLinkText = HttpUtility.HtmlDecode(parsedLinkText);

                raw_list.Add(parsedLinkText);

                links_list.Add(parsedLinkAddress);
            }

            raw_list.Remove("О ПЕРЕЗАЛИВЕ ТОРРЕНТ-ФАЙЛОВ");
            raw_list.Remove("О РАЗМЕЩЕНИИ НОВЫХ РЕЛИЗОВ");
            raw_list.RemoveAll(item => item == "О РАЗМЕЩЕНИИ РЕЛИЗОВ");

            links_list.Remove("4494103"); // О РАЗМЕЩЕНИИ НОВЫХ РЕЛИЗОВ     2015

            links_list.Remove("1719531"); // О ПЕРЕЗАЛИВЕ ТОРРЕНТ-ФАЙЛОВ    2011-2014
            links_list.Remove("1719477"); // О РАЗМЕЩЕНИИ НОВЫХ РЕЛИЗОВ     2011-2014

            links_list.Remove("1719529"); // О ПЕРЕЗАЛИВЕ ТОРРЕНТ-ФАЙЛОВ    2006-2010
            links_list.Remove("1719475"); // О РАЗМЕЩЕНИИ РЕЛИЗОВ           2006-2010

            links_list.Remove("1719528"); // О ПЕРЕЗАЛИВЕ ТОРРЕНТ-ФАЙЛОВ    2001-2005
            links_list.Remove("1719471"); // О РАЗМЕЩЕНИИ РЕЛИЗОВ           2001-2005

            links_list.Remove("3754455"); // О ПЕРЕЗАЛИВЕ ТОРРЕНТ-ФАЙЛОВ    1991-2000
            links_list.Remove("3754459"); // О РАЗМЕЩЕНИИ НОВЫХ РЕЛИЗОВ     1991-2000

            int linkIndex = 0;

            foreach (var listItem in raw_list)
            {
                string input = listItem.ToString();

    #region NameRussian
                string nameRussian = "";

                int indexNameRussian = input.IndexOf("/");

                if (indexNameRussian != -1)
                {
                    nameRussian = input.Substring(0, indexNameRussian);
                } 
    #endregion

    #region videoQuality

                string videoQuality = "";

                foreach (var item in videoQuality_list)
                {
                    //videoQuality = item;

                    if (input.ToLower().Contains(item.ToLower()))
                    {
                        videoQuality = item;
                        break;
                    }

                    else
                    {
                        videoQuality = "~N/A";
                    }
                }
    #endregion

    #region audioQuality

                string audioQuality = "";

                foreach (var item in audioQuality_list)
                {
                    if (input.ToLower().Contains(item.ToLower()))
                    {
                        audioQuality = item;
                        break;
                    }

                    else
                    {
                        audioQuality = "~N/A";
                    }
                }
    #endregion

    #region Year
                int indexYear = input.IndexOf(") [") + 2;
                string year = "";

                if (indexYear != -1)
                {
                    year = input.Replace(" ", string.Empty);
                    indexYear = year.IndexOf(")[") + 1;
                    year = year.Substring(indexYear + 1, 4);
                } 
    #endregion

    #region Name
                int indexBracket = input.IndexOf("(");

                if (indexBracket != -1)
                {
                    input = input.Substring(0, indexBracket);
                }

                int indexSlash = input.IndexOf("/");

                while (indexSlash != -1)
                {
                    input = input.Substring(indexSlash + 2);

                    indexSlash = input.IndexOf("/");
                } 
    #endregion

                parsed_list.Add(input);
                               
                string linkAddress = links_list[linkIndex];
                linkIndex++;

                if (nameRussian == "" && input != "")
                {
                    nameRussian = input;
                    input = "";
                }

                final_list.Add(new Movie {  Name = input,
                                            Year = year,
                                            Rating = string.Empty,
                                            Link = linkAddress,
                                            videoQuality = videoQuality,
                                            audioQuality = audioQuality,
                                            NameRussian = nameRussian
                                         });
                
            }

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = progressBar1.Maximum;

            this.IsEnabled = true;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            GetIMBDRating(worker);
        }

        private void rating_btn_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += worker_DoWork;

            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (s, e1) =>
            { progressBar1.Value = e1.ProgressPercentage; };
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();

            this.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            this.MinHeight = PrimaryScreenHeight * 0.9;
            //this.MaxHeight = PrimaryScreenHeight * 0.95;

            this.MinWidth = 900;           

            List<RutrackerLink> links = new List<RutrackerLink>();

            string baseLink = "http://rutracker.org/forum/viewforum.php?f=";        
            
            links.Add(new RutrackerLink { ID = "1991-2000", Link = baseLink + "2221" });
            links.Add(new RutrackerLink { ID = "2001-2005", Link = baseLink + "2091" });
            links.Add(new RutrackerLink { ID = "2006-2010", Link = baseLink + "2092" });
            links.Add(new RutrackerLink { ID = "2011-2014", Link = baseLink + "2093" });
            links.Add(new RutrackerLink { ID = "2015", Link = baseLink + "2200" });

            comboBox1.ItemsSource = links;
            comboBox1.DisplayMemberPath = "ID";
            comboBox1.SelectedValuePath = "Link";
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.SizeToContent = SizeToContent.Width;
            }
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            progressBar1.Value = 0;
            
            rating_btn.IsEnabled = true;

            ClearAll();

            years_list.Clear();

            var selectedItem = comboBox1.SelectedItem as RutrackerLink;

            string ID = selectedItem.ID.ToString();

            int indexDash = ID.IndexOf('-');
            int year1 = 0;
            int year2 = 0;

            if (indexDash != -1)
            {
                year1 = int.Parse(ID.Substring(0, indexDash));
                year2 = int.Parse(ID.Substring(indexDash+1));
            }

            if (ID == year1 + "-" + year2)
            {
                for (int i = year1; i <= year2; i++)
                {
                    years_list.Add(i.ToString());
                }

            }

            else
            {
                if (selectedItem.ID == "2015")
                {
                    years_list.Add("2015");
                }
            }

            listBox_years.ItemsSource = null;
            listBox_years.ItemsSource = years_list;

            textBox1.Text = string.Empty;
            textBox1.Text = "1";
        }

        private void undoFilter_btn_Click(object sender, RoutedEventArgs e)
        {
            dataGrid1.ItemsSource = null;
            dataGrid1.ItemsSource = final_list;
        }

        DispatcherTimer popupTimer = new DispatcherTimer(DispatcherPriority.Normal);

        Point cursorPositionInit = new Point();

        private void ShowPopUp(object sender, RoutedEventArgs e)
        {
            cursorPositionInit = Mouse.GetPosition(this);

            popUp.HorizontalOffset = 0;
            popUp.VerticalOffset = 0;

            var margin = 7;

            moviePoster.Source = null;
            var dataGridRow = e.Source as DataGridRow;
            var selectedItem = dataGridRow.Item as Movie;

            movieName.MaxWidth = 100;
            movieName.Text = selectedItem.Name;
            movieName.Margin = new Thickness(margin);

            movieYear.Text = selectedItem.Year;
            movieYear.Margin = new Thickness(margin, 0, margin, margin);

            if (selectedItem.Poster != null && selectedItem.Poster != string.Empty && selectedItem.Poster != "N/A")
            {
                BitmapImage logo = new BitmapImage();
                logo.BeginInit();
                logo.UriSource = new Uri(selectedItem.Poster);                
                logo.EndInit();
                moviePoster.Source = logo;
                moviePoster.Margin = new Thickness(margin, 0, margin, margin);
                moviePoster.MaxWidth = 100;
            }

            moviePlot.MaxWidth = 120;
            moviePlot.Text = selectedItem.Plot;
            moviePlot.Margin = new Thickness(margin/2, margin, margin, margin);                        

            popUp.PlacementTarget = dataGridRow;
            popUp.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
            
            popupTimer.Interval = TimeSpan.FromMilliseconds(1000);
            popupTimer.Tick += (obj, args) =>
            {
                popUp.IsOpen = true;
            };
            popupTimer.Start();
        }

        private void HidePopUp(object sender, RoutedEventArgs e)
        {
            popUp.IsOpen = false;
            popupTimer.Stop();
        }

        private void MovePopUp(object sender, RoutedEventArgs e)
        {
            var cursorPositionNow = Mouse.GetPosition(this);

            var deltaPoint = cursorPositionNow - cursorPositionInit;

            popUp.HorizontalOffset += deltaPoint.X;
            popUp.VerticalOffset += deltaPoint.Y;

            cursorPositionInit = cursorPositionNow;
        }

        int page;

        private void previous_btn_Click(object sender, RoutedEventArgs e)
        {
            if (page != 0)
            {
                page -= 50;
                textBox1.Text = (1 + page / 50).ToString();
            }
        }

        private void next_btn_Click(object sender, RoutedEventArgs e)
        {
            page += 50;
            textBox1.Text = (1 + page / 50).ToString();           
        }

        private void select_all_qualities_btn_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_videoQuality.SelectedItems.Count == listBox_videoQuality.Items.Count)
            {
                listBox_videoQuality.SelectedItems.Clear();
            }
            else
            {
                listBox_videoQuality.SelectAll();
            }
        }

        private void select_all_years_btn_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_years.SelectedItems.Count == listBox_years.Items.Count)
            {
                listBox_years.SelectedItems.Clear();
            }
            else
            {
                listBox_years.SelectAll();
            }
        }

        private void button_selectAllAudio_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_audioQuality.SelectedItems.Count == listBox_audioQuality.Items.Count)
            {
                listBox_audioQuality.SelectedItems.Clear();
            }
            else
            {
                listBox_audioQuality.SelectAll();
            }
        } 

        private void listBox_filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            applyFilters();
        }

        private void textBox_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            applyFilters();
        }

        private void menuItemClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        public void menuItemOptions_Click(object sender, RoutedEventArgs e)
        {
            Window2 optionsWindow = new Window2();
            optionsWindow.Owner = this;
            optionsWindow.Show();
        }

        public void menuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            Window3 aboutWindow = new Window3();            
            aboutWindow.Owner = this;
            aboutWindow.Show();
        }

        private void button_debugWindow_Click(object sender, RoutedEventArgs e)
        {
            Debug debugWindow = new Debug();
            debugWindow.Owner = this;
            debugWindow.Show();
        }

        public void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox1.Text.Trim() != "")
            {
                int validPageNumber = 0;
                int.TryParse(textBox1.Text, out validPageNumber);

                if (validPageNumber != 0)
                {
                    page = (int.Parse(textBox1.Text) - 1) * 50;

                    if (page <= 0)
                    {
                        textBox1.Text = "1";
                    }

                    pagesHandling();
                }
                else
                {
                    textBox1.Text = "1";
                }
            }

            else
            {
                textBox1.Text = "1";
            }
        }

        private void textBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            textBox1.SelectAll();
        }

        private void textBox1_GotMouseCapture(object sender, MouseEventArgs e)
        {
            textBox1.SelectAll();
        }

        private void dataGrid1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid1.SelectedItem != null && dataGrid1.SelectedItems.Count == 1)
            {
                var currentColumn = dataGrid1.CurrentCell.Column.Header.ToString();
                if (currentColumn == "Link")
                {
                    var cellContent = dataGrid1.SelectedItem as Movie;
                    string webLink = "http://rutracker.org/forum/viewtopic.php?t=" + cellContent.Link.ToString();

                    openWebLink(webLink);
                }
            }
        }
    }
}

