using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MyWindowsMediaPlayerV2.Medias;
using MyWindowsMediaPlayerV2.Model;

namespace MyWindowsMediaPlayerV2
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String actualOrder = "Title";
        private Boolean isPlaying = false;
        private Boolean filePlaying = false;
        private Boolean statut = false;
        private ServiceLib lib;
        public MainWindow()
        {
            Uri iconUri = new Uri("pack://application:,,,/icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            InitializeComponent();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.AddExtension = true;
            if (statut == false)
            {
                ofd.Filter = "Media Files (*.mp3,*.wav,*.wma,*.mp4*.wmv,*.avi,*.jpg,*.jpeg)|*.mp3;*.wav;*.wma;*.mp4;*.wmv;*.avi;*.jpg;*.jpeg";
            }
            else
            {
                ofd.Filter = "Media Files (*.mp3,*.wav,*.wma,*.mp4,*.avi)|*.mp3;*.wav;*.wma;*.mp4;*.avi";
            }
            ofd.ShowDialog();
            if (statut == false && ofd.FileName != "")
            {
                try
                {
                    TagLib.File file = TagLib.File.Create(ofd.FileName);
                    Media tmp = new Media();
                    tmp.title = file.Tag.Title;
                    tmp.album = file.Tag.Album;
                    tmp.artist = file.Tag.Performers.ToList<String>();
                    tmp.path = ofd.FileName;
                    if (tmp.title == null)
                    {
                        tmp.title = ofd.SafeFileName;
                    }
                    if (tmp.album == null)
                    {
                        tmp.album = "N/A";
                    }
                    if (tmp.artist.Count == 0)
                    {
                        tmp.artist.Add("N/A");
                    }
                    listView.Items.Add(tmp);
                }
                catch
                {
                    new NullReferenceException("Error while opening media file");
                }
            }
            else if (statut == true && ofd.FileName != "")
            {
                lib.addMusic(ofd.FileName);
                listView.Items.Add(lib.library.songs.Last());
            }
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(timer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }


        void timer_Tick(object sender, EventArgs e)
        {
            slider.Value = mediaElement.Position.TotalSeconds;
        }

        private void stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = filePlaying;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            isPlaying = false;
        }

        private void playpause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mediaElement != null) && (mediaElement.Source != null);
        }

        private void playpause_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying == false)
            {
                mediaElement.Play();
                filePlaying = true;
                isPlaying = true;
            }
            else if (isPlaying == true)
            {
                mediaElement.Pause();
                isPlaying = false;
            }
        }

        private void del_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (listView.SelectedIndex != -1);
        }

        private void del_Playlist(object sender, RoutedEventArgs e)
        {
            int lastIndex;
            if (statut == false)
            {
                mediaElement.Source = null;
                lastIndex = listView.SelectedIndex;
                listView.Items.RemoveAt(listView.SelectedIndex);
            }
            else
            {
                Song tmp = (Song)listView.SelectedItem;
                mediaElement.Source = null;
                lastIndex = listView.SelectedIndex;
                listView.Items.RemoveAt(listView.SelectedIndex);
                lib.delSong(tmp, false);
            }
            if (listView.Items.Count != 0 && lastIndex == 0)
                listView.SelectedIndex = lastIndex;
            else
                listView.SelectedIndex = lastIndex - 1;
        }


        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan ts = TimeSpan.FromSeconds(e.NewValue);
            mediaElement.Position = ts;
            time.Text = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
        }

        private void volume_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = volume_Slider.Value;
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                slider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            }
        }
        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mediaElement.Stop();
            isPlaying = false;

            if (statut == false && listView.SelectedItem != null)
            {
                Media url = (Media)listView.SelectedItem;
                try
                {
                    mediaElement.Source = new Uri(url.path);
                }
                catch
                {
                    Console.WriteLine("Error mediaElement");
                }
            }
            else if (statut == true && listView.SelectedItem != null)
            {
                Song url;
                try
                {
                    url = (Song)listView.SelectedItem;
                }
                catch
                {
                    listView.Items.Clear();
                    url = new Song();
                }
                if (url.path != null)
                {
                    try
                    {
                        mediaElement.Source = new Uri(url.path);
                    }
                    catch
                    {
                        Console.WriteLine("Error mediaElement");
                    }
                }
            }
        }

        private void next_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (listView != null)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            int index = listView.SelectedIndex + 1;

            if (index < listView.Items.Count)
            {
                listView.SelectedIndex = index;
                if (statut == false)
                {
                    Media url = (Media)listView.SelectedItem;
                    if (url.path != null)
                    {
                        mediaElement.Source = new Uri(url.path);
                        mediaElement.Play();
                        filePlaying = true;
                        isPlaying = true;
                    }
                }
                else
                {
                    Song url = (Song)listView.SelectedItem;
                    if (url.path != null)
                    {
                        mediaElement.Source = new Uri(url.path);
                        mediaElement.Play();
                        filePlaying = true;
                        isPlaying = true;
                    }
                }
            }
        }

        private void previous_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (listView != null)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void previous_Click(object sender, RoutedEventArgs e)
        {
            int index = listView.SelectedIndex - 1;
            if (index >= 0)
            {
                listView.SelectedIndex = index;
                if (statut == false)
                {
                    Media url = (Media)listView.SelectedItem;
                    if (url.path != null)
                    {
                        mediaElement.Source = new Uri(url.path);
                        mediaElement.Play();
                        filePlaying = true;
                        isPlaying = true;
                    }
                }
                else
                {
                    Song url = (Song)listView.SelectedItem;
                    if (url.path != null)
                    {
                        mediaElement.Source = new Uri(url.path);
                        mediaElement.Play();
                        filePlaying = true;
                        isPlaying = true;
                    }
                }
            }
        }

        private void playlist_Choice(object sender, RoutedEventArgs e)
        {
            statut = false;
            listView.DataContext = null;
            listView.ItemsSource = null;
            listView.Items.Clear();
            delete_list();
            mediaElement.Source = null;
            playlist.FontWeight = FontWeights.Bold;
            library.FontWeight = FontWeights.Normal;
        }

        private void lib_Choice(object sender, RoutedEventArgs e)
        {
            statut = true;
            int i = 0;
            listView.Items.Clear();
            listView.DataContext = null;
            listView.ItemsSource = null;
            delete_list();
            mediaElement.Source = null;
            lib = new ServiceLib(new Library());
            while (i != lib.library.songs.Count())
            {
                listView.Items.Add(lib.library.songs.ElementAt(i));
                i += 1;
            }
            playlist.FontWeight = FontWeights.Normal;
            library.FontWeight = FontWeights.Bold;
        }
        private void delete_list()
        {
            int i = 0;

            while (i != listView.Items.Count)
            {
                listView.Items.RemoveAt(i);
                i += 1;
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            listView.Items.Clear();

            var res = lib.library.songs.Where(i => string.IsNullOrEmpty(textBox.Text)
            || i.title.StartsWith(textBox.Text)
            || i.artist[0].StartsWith(textBox.Text)
            || i.album.StartsWith(textBox.Text))
            .Select(c => new Song(c.title, c.artist, c.album, c.path, c.id));

            foreach (Song x in res)
            {
                listView.Items.Add(x);
            }
        }

        private void switch_songs(int idx)
        {
            var temp = listView.Items[idx];

            listView.Items[idx] = listView.Items[idx + 1];
            listView.Items[idx + 1] = temp;
        }

        private bool compare_medias_by_order(string order, Media first, Media second)
        {
            if (order == "Title")
                return (first.title.CompareTo(second.title) > 0);
            else if (order == "Album")
                return (first.album.CompareTo(second.album) > 0);
            else
                return (first.artist[0].CompareTo(second.artist[0]) > 0);
        }

        private bool compare_songs_by_order(string order, Song first, Song second)
        {
            if (order == "Title")
                return (first.title.CompareTo(second.title) > 0);
            else if (order == "Album")
                return (first.album.CompareTo(second.album) > 0);
            else
                return (first.artist[0].CompareTo(second.artist[0]) > 0);
        }

        private void reverse_songs_list()
        {
            int s = listView.Items.Count;
            int i = 0;

            while (i < s / 2)
            {
                var temp = listView.Items[i];

                listView.Items[i] = listView.Items[s - (i + 1)];
                listView.Items[s - (i + 1)] = temp;
                i += 1;
            }
        }

        private void order_songs_list(String order)
        {
            bool isSorted = true;

            for (int i = 0; i + 1 < listView.Items.Count; i++)
            {
                if ((!statut && compare_medias_by_order(order, listView.Items[i] as Media, listView.Items[i + 1] as Media)) ||
                    (statut && compare_songs_by_order(order, (Song)listView.Items[i], (Song)listView.Items[i + 1])))
                {
                    isSorted = false;
                    switch_songs(i);
                }
            }
            if (!isSorted)
                order_songs_list(order);
        }

        private void header_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
            e.OriginalSource as GridViewColumnHeader;

            if (headerClicked != null)
            {
                var orderBy = headerClicked.Column.Header as string;

                if (orderBy != actualOrder)
                {
                    actualOrder = orderBy;
                    order_songs_list(orderBy);
                }
                else
                    reverse_songs_list();
            }
        }
    }
}
