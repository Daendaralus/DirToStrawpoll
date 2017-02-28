using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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

namespace DirToList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private object dummyNode = null;
        ObservableCollection<CheckBoxListViewItemSource> itemSources;
        public MainWindow()
        {
            InitializeComponent();
            itemSources = new ObservableCollection<CheckBoxListViewItemSource>();
            lsItems.ItemsSource = itemSources;
        }

        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                foldersItem.Items.Add(item);
            }
        }
        public string SelectedImagePath { get; set; }
        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);

            if (temp == null)
                return;
            SelectedImagePath = "";
            string temp1 = "";
            string temp2 = "";
            while (true)
            {
                temp1 = temp.Header.ToString();
                if (temp1.Contains(@"\"))
                {
                    temp2 = "";
                }
                SelectedImagePath = temp1 + temp2 + SelectedImagePath;
                if (temp.Parent.GetType().Equals(typeof(TreeView)))
                {
                    break;
                }
                temp = ((TreeViewItem)temp.Parent);
                temp2 = @"\";
            }
            memesWeow();
            
            //show user selected path
            //MessageBox.Show(SelectedImagePath);
        }

        private void cpyClip_Click(object sender, RoutedEventArgs e)
        {
            List<string> memes = itemSources.Where((cb) => cb.IsChecked).Select((cb)=>cb.Text).ToList();
            memes = memes.Select((st) => st = st.Split('\\').Last()).ToList();
            string s = cbClipSep.IsChecked==true ? "\n" : ", ";
            Clipboard.SetText(String.Join(s,memes));
        }

        private void CreateStrawPoll_Click(object sender, RoutedEventArgs e)
        {
            List<string> memes = itemSources.Where((cb) => cb.IsChecked).Select((cb) => cb.Text).ToList();
            if(memes.Count>30)
            {
                MessageBox.Show("Options are limited to 30. Deselect a few and try again.");
            }
            var s = String.Format("[\"{0}\"]", String.Join("\",\"", memes));
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("Content-Type: application/json");
                    //s = "[\"test\",\"test2\"]";
                    //byte[] r2 = client.DownloadData("https://strawpoll.me/api/v2/polls/1");
                    //string res = System.Text.Encoding.UTF8.GetString(r2);
                    var memoirs = "{\"title\":\"" + tbTitle.Text + "\",\"options\":" + s + ",\"multi\":\"" + cbMulti.IsChecked.ToString().ToLower() + "\",\"dupcheck\":\""+((ComboBoxItem)cbDup.SelectedValue).Content.ToString().ToLower()+"\",\"captcha\":\"" + cbCaptcha.IsChecked.ToString().ToLower() + "\"}";
                    string response =
                    client.UploadString("https://strawpoll.me/api/v2/polls", memoirs);
                    if (response.Contains("\"id\":"))
                    {
                        response = response.Trim('{').Trim('}').Split(',')[0].Split(':').Last().Trim('"');
                        Clipboard.SetText("https://strawpoll.me/" + response);
                        tbStrawpoll.Text = "https://strawpoll.me/" + response;
                    }
                    //    new NameValueCollection()
                    //{
                    //    { "title", "Moobers" },
                    //    { "options", "[\"meme1\",\"meme2\"]"},
                    //    {"multi", "true"},
                    //    {"dupcheck", "normal"},
                    //    {"captcha", "false"}
                    //});

                    //string result = System.Text.Encoding.UTF8.GetString(response);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            GridView gridView = lsItems.View as GridView;
            if (gridView != null && gridView.Columns.Count >= 2)
            {
                // Calculate the item’s desired text width and increase the
                // text column’s width to match the widest text
                TextBlock tb = (TextBlock)sender;
                tb.Measure(new Size(Double.MaxValue, Double.MaxValue));
                double newWidth = tb.DesiredSize.Width;
                GridViewColumnCollection columns = gridView.Columns;
                if (newWidth > columns[1].Width ||
                    double.IsNaN(columns[1].Width))
                {
                    columns[1].Width = newWidth;
                }
                // Remove the text block cell’s content presenter built-in
                // margin for better-looking spacing
                ContentPresenter contentPresenter = VisualTreeHelper.GetParent(tb) as ContentPresenter;
                if (contentPresenter != null)
                {
                    contentPresenter.Margin = new Thickness(0);
                }
            }
        }

        private void cbToggleAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach(var t in itemSources)
            {
                t.IsChecked = true;
            }
        }

        private void cbToggleAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var t in itemSources)
            {
                t.IsChecked = false;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            tbCount.Text = itemSources.Count((cb) => cb.IsChecked).ToString();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            itemSources.First((cb) => cb.Text == ((TextBlock)sender).Text).Toggle();
            tbCount.Text = itemSources.Count((cb) => cb.IsChecked).ToString();
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
                itemSources.First((cb) => cb.Text == ((TextBlock)sender).Text).Toggle();
            tbCount.Text = itemSources.Count((cb) => cb.IsChecked).ToString();
        }

        private void cbFiles_Click(object sender, RoutedEventArgs e)
        {
            memesWeow();
            
        }

        private void cbDir_Click(object sender, RoutedEventArgs e)
        {
            memesWeow();
            
        }

        public void memesWeow()
        {
            List<string> memes = new List<string>();
            try
            {
                if (cbDir.IsChecked == true)
                    memes.AddRange(Directory.EnumerateDirectories(SelectedImagePath));
                if (cbFiles.IsChecked == true)
                    memes.AddRange(Directory.EnumerateFiles(SelectedImagePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            itemSources.Clear();
            cbToggleAll.IsChecked = true;
            memes.ForEach((val) => itemSources.Add(new CheckBoxListViewItemSource(val.Split('\\').Last())));
            //memes.Select((st) => {itemSources.Add(new CheckBoxListViewItemSource(st.Split('\\').Last())); return st;});
            tbCount.Text = itemSources.Count.ToString();
        }
    }
    #region HeaderToImageConverter
    [ValueConversion(typeof(string), typeof(bool))]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance =
            new HeaderToImageConverter();

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if ((value as string).Contains(@"\"))
            {
                Uri uri = new Uri
                ("pack://application:,,,/Images/diskdrive.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            }
            else
            {
                Uri uri = new Uri("pack://application:,,,/Images/folder.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
    #endregion
    public class CheckBoxListViewItemSource : INotifyPropertyChanged
    {
        public CheckBoxListViewItemSource(String text)
        {
            m_text = text;
            m_checked = true;
        }

        public bool IsChecked
        {
            get { return m_checked; }
            set
            {
                if (m_checked == value) return;
                m_checked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        public void Toggle()
        {
            IsChecked = !IsChecked;
        }
        public String Text
        {
            get { return m_text; }
            set
            {
                if (m_text == value) return;
                m_text = value;
                RaisePropertyChanged("Text");
            }
        }

        public override string ToString()
        {
            return Text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            PropertyChangedEventHandler eh = PropertyChanged;
            if (eh != null)
            {
                eh(this, new PropertyChangedEventArgs(propName));
            }
        }

        private bool m_checked;
        private String m_text;
    }
}
