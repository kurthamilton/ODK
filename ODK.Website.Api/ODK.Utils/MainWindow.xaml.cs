using System.Windows;

namespace ODK.Utils
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ImageDownloader.DownloadImages(txtData.Text, txtSavePath.Text);
        }
    }
}
