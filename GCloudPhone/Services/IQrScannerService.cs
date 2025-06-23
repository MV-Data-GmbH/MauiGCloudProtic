using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using GCloudShared.Interface;

namespace GCloudPhone.Services
{
    public interface IQrScannerService
    {
        Task StartScanningAsync(Action<string> onScanned);
        Task StartScanningAsync(Action<string> onScanned, View previewContainer);
        Task StartScanningAsync(Action<string> onScanned, View previewContainer, IAuthService authService);
        void StopScanning();
    }
}