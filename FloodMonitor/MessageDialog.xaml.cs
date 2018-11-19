using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace FloodMonitor
{
    /// <summary>
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : UserControl
    {
        public MessageDialog()
        {
            InitializeComponent();
        }

        public async Task<bool> Show()
        {
            var res = await DialogHost.Show(this, "Root");
            return (res as bool?) ?? false;
        }

        public static async Task<bool> Show(string title, string message = "", string positive = "_OKAY", string negative = "")
        {
            var dlg = new MessageDialog();
            if (string.IsNullOrEmpty(negative))
                dlg.Negative.Visibility = Visibility.Collapsed;
            dlg.Negative.Content = negative;
            dlg.Positive.Content = positive;
            dlg.Message.Text = message;
            dlg.Title.Text = title;
            return await dlg.Show();
        }
    }
}
