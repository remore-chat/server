using System.Threading.Tasks;

namespace TTalk.WinUI.Contracts.Services
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }
}
