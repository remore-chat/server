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
    }
}
