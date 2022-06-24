using System.Threading.Tasks;

namespace Remore.WinUI.Contracts.Services
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }
}
