using System;

namespace Remore.WinUI.Contracts.Services
{
    public interface IPageService
    {
        Type GetPageType(string key);
    }
}
