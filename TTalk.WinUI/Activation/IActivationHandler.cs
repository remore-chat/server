using System.Threading.Tasks;

namespace TTalk.WinUI.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle(object args);

        Task HandleAsync(object args);
    }
}
