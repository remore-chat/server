using Remore.WinUI.ViewModels.Dialog;
using Remore.WinUI.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remore.WinUI.Factories
{
    public interface IDialogFactory
    {
        JoinServerDialog CreateJoinServerDialog();
        NotificationDialog CreateNotificationDialog(string title, object content, string primaryButtonText = null);
    }

    public class DialogFactory : IDialogFactory
    {
        public JoinServerDialog CreateJoinServerDialog()
        {
            var dialog = new JoinServerDialog()
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
            };
            return dialog;
        }

        public NotificationDialog CreateNotificationDialog(string title, object content, string primaryButtonText = null)
        {
            var vm = new NotificationDialogViewModel(title, content, primaryButtonText);
            var dialog = new NotificationDialog(vm)
            {
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            return dialog;
        }
    }
}
