using System.Threading.Tasks;

namespace Remore.WinUI.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle(object args);

        Task HandleAsync(object args);
    }
}
