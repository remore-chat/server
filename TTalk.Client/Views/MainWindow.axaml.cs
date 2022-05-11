using Avalonia;
using Avalonia.Controls;
using System.Threading.Tasks;

namespace TTalk.Client.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }


        public async Task<object?> ShowDialogHost(object model, string view)
        {
            return await DialogHost.DialogHost.Show(model, view);
        }

    }
}
