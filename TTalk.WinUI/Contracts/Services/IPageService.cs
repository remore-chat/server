using System;

namespace TTalk.WinUI.Contracts.Services
{
    public interface IPageService
    {
        Type GetPageType(string key);
    }
}
