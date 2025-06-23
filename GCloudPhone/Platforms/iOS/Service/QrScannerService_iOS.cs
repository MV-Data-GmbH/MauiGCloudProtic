#if IOS 
using System;
using System.Linq;
using System.Threading.Tasks;
using AVFoundation;
using CoreFoundation;
using Foundation;
using UIKit;
using GCloudPhone.Services;
using CoreGraphics;
using Microsoft.Maui.Controls; // Za View, itd.
using Microsoft.Maui.ApplicationModel; // Za MainThread
using GCloudPhone.Views; // Pretpostavljamo da OrderTypePage pripada ovom namespace-u
using GCloudShared.Interface;
using GCloudPhone.Views.Shop;
using GCloudPhone.Views.Shop.OrderProccess; // Za IAuthService

namespace GCloudPhone.Platforms.iOS
{
    public class QrScannerService_iOS : IQrScannerService
    {
        AVCaptureSession captureSession;
        AVCaptureMetadataOutput metadataOutput;
        Action<string> onScannedCallback;
        AVCaptureVideoPreviewLayer previewLayer;

        // Atributi za overlay, back dugme i logiku stabilnosti
        UIView yellowOverlay;
        UIButton backButton;
        string lastDetectedValue;
        DateTime? stableStartTime; // Vreme prve detekcije istog koda

        // Overload 1: Jedan parametar
        public async Task StartScanningAsync(Action<string> onScanned)
        {
            await StartScanningAsync(onScanned, null, null);
        }

        // Overload 2: Dva parametra – implementacija iz interfejsa
        public async Task StartScanningAsync(Action<string> onScanned, View previewContainer)
        {
            // Ako se pozove metoda sa dva parametra, pribavljamo IAuthService preko DependencyService
            // (ako je to ono što vam radi, inače se prosledi kroz DI)
            var authService = DependencyService.Get<IAuthService>();
            await StartScanningAsync(onScanned, previewContainer, authService);
        }

        // Overload 3: Tri parametra – proširena verzija koja prima i instancu IAuthService
        public async Task StartScanningAsync(Action<string> onScanned, View previewContainer, IAuthService authService)
        {
            // Resetovanje stanja
            lastDetectedValue = null;
            stableStartTime = null;

            onScannedCallback = onScanned;
            Console.WriteLine("[QRService] Starting scanning process...");

            // Provera autorizacije kamere
            var authStatus = AVCaptureDevice.GetAuthorizationStatus((AVAuthorizationMediaType)AVMediaTypes.Video);
            Console.WriteLine("[QRService] Authorization status: " + authStatus);
            if (authStatus != AVAuthorizationStatus.Authorized)
            {
                bool granted = await AVCaptureDevice.RequestAccessForMediaTypeAsync((AVAuthorizationMediaType)AVMediaTypes.Video);
                Console.WriteLine("[QRService] Request access result: " + granted);
                if (!granted)
                {
                    Console.WriteLine("[QRService] Camera access permission not granted.");
                    return;
                }
            }

            // Konfigurisanje capture session-a
            captureSession = new AVCaptureSession();
            NSError error;
            var videoDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            if (videoDevice == null)
            {
                Console.WriteLine("[QRService] Video device not found.");
                return;
            }
            Console.WriteLine("[QRService] Video device found: " + videoDevice.LocalizedName);
            var videoInput = new AVCaptureDeviceInput(videoDevice, out error);
            if (error != null)
            {
                Console.WriteLine($"[QRService] Error creating video input: {error.LocalizedDescription}");
                return;
            }
            if (captureSession.CanAddInput(videoInput))
            {
                captureSession.AddInput(videoInput);
                Console.WriteLine("[QRService] Video input added.");
            }
            else
            {
                Console.WriteLine("[QRService] Cannot add video input to the session.");
                return;
            }

            // Konfigurisanje metadata output-a
            metadataOutput = new AVCaptureMetadataOutput();
            if (captureSession.CanAddOutput(metadataOutput))
            {
                captureSession.AddOutput(metadataOutput);
                Console.WriteLine("[QRService] Metadata output added.");
            }
            else
            {
                Console.WriteLine("[QRService] Cannot add metadata output to the session.");
                return;
            }

            // Postavljanje delegate-a – delegate ažurira detekciju QR koda
            var metadataDelegate = new QrMetadataOutputDelegate(onScannedCallback, captureSession, this);
            metadataOutput.SetDelegate(metadataDelegate, DispatchQueue.MainQueue);

            // Postavljanje vrste metapodatka
            string targetType = "org.iso.QRCode";
            Console.WriteLine("[QRService] Setting WeakMetadataObjectTypes to '" + targetType + "'");
            metadataOutput.WeakMetadataObjectTypes = new NSString[] { new NSString(targetType) };

            // Određivanje container-a za preview layer
            UIView containerView;
            if (previewContainer != null)
            {
                containerView = previewContainer.Handler?.PlatformView as UIView;
                if (containerView == null)
                {
                    Console.WriteLine("[QRService] Unable to convert MAUI view to native UIView; using key window instead.");
                    containerView = GetDefaultContainerView();
                }
            }
            else
            {
                containerView = GetDefaultContainerView();
            }
            Console.WriteLine("[QRService] Container bounds: " + containerView.Bounds);

            // Kreiranje i konfiguracija preview layer-a
            previewLayer = new AVCaptureVideoPreviewLayer(captureSession)
            {
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };
            containerView.Layer.AddSublayer(previewLayer);
            containerView.SetNeedsLayout();
            containerView.LayoutIfNeeded();
            previewLayer.Frame = containerView.Bounds;
            Console.WriteLine("[QRService] Preview layer frame: " + previewLayer.Frame);

            // Kreiranje žutog overlay-a (frejma)
            var overlaySize = new nfloat(200);
            var overlayX = (containerView.Bounds.Width - overlaySize) / 2;
            var overlayY = (containerView.Bounds.Height - overlaySize) / 2;
            var overlayFrame = new CGRect(overlayX, overlayY, overlaySize, overlaySize);
            yellowOverlay = new UIView(overlayFrame)
            {
                BackgroundColor = UIColor.Clear
            };
            yellowOverlay.Layer.BorderColor = UIColor.Yellow.CGColor;
            yellowOverlay.Layer.BorderWidth = 2.0f;
            containerView.AddSubview(yellowOverlay);
            containerView.BringSubviewToFront(yellowOverlay);
            Console.WriteLine("[QRService] Yellow overlay added at: " + overlayFrame);

            // Dodavanje back dugmeta u korenski view koristeći prosleđenu instancu authService
            AddBackButtonToRoot(authService);

            // Startovanje capture session-a
            captureSession.StartRunning();
            Console.WriteLine("[QRService] Capture session started.");
        }

        /// <summary>
        /// Dodaje back bar u korenski view (odmah ispod status bara u gornjem levom uglu).
        /// Back bar ima pozadinsku boju "#ffd401", padding, a dugme "Zurück" ima tekst boje "#CC0719".
        /// Klikom se zaustavlja skeniranje i navigira na OrderType stranicu koristeći prosleđenu instancu IAuthService.
        /// </summary>
        private void AddBackButtonToRoot(IAuthService authService)
        {
            UIView rootView = GetDefaultContainerView();

            // Određivanje safe area – koristimo SafeAreaInsets ili fallback na StatusBarFrame.Height/20pt
            nfloat safeTop = rootView.SafeAreaInsets.Top;
            if (safeTop == 0)
                safeTop = UIApplication.SharedApplication.StatusBarFrame.Height;
            if (safeTop == 0)
                safeTop = 20; // fallback

            nfloat barHeight = 60;
            UIView backBar = new UIView(new CGRect(0, safeTop, rootView.Bounds.Width, barHeight))
            {
                BackgroundColor = UIColor.FromRGB(0xFF, 0xD4, 0x01) // #ffd401
            };

            nfloat paddingLeft = 10;
            nfloat paddingTop = 10;
            nfloat buttonWidth = 100; // prilagodite širinu po potrebi
            nfloat buttonHeight = barHeight - 2 * paddingTop;

            backButton = new UIButton(UIButtonType.System)
            {
                Frame = new CGRect(paddingLeft, paddingTop, buttonWidth, buttonHeight)
            };
            backButton.SetTitle("Zurück", UIControlState.Normal);
            backButton.BackgroundColor = UIColor.Clear;
            backButton.SetTitleColor(UIColor.FromRGB(0xCC, 0x07, 0x19), UIControlState.Normal); // #CC0719
            backButton.TouchUpInside += (sender, args) =>
            {
                Console.WriteLine("[QRService] Back button tapped.");
                StopScanning();
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Koristimo prosleđenu instancu authService za navigaciju
                    await Application.Current.MainPage.Navigation.PushAsync(new OrderTypePage(authService));
                });
            };

            backBar.AddSubview(backButton);
            rootView.AddSubview(backBar);
            rootView.BringSubviewToFront(backBar);
            Console.WriteLine("[QRService] Back button added to root view.");
        }

        /// <summary>
        /// Zaustavlja skeniranje i uklanja preview layer, yellow overlay i back bar (sa dugmetom).
        /// </summary>
        public void StopScanning()
        {
            if (captureSession != null && captureSession.Running)
            {
                captureSession.StopRunning();
                Console.WriteLine("[QRService] Capture session stopped.");
            }
            if (previewLayer != null)
            {
                previewLayer.RemoveFromSuperLayer();
                previewLayer = null;
                Console.WriteLine("[QRService] Preview layer removed.");
            }
            if (yellowOverlay != null)
            {
                yellowOverlay.RemoveFromSuperview();
                yellowOverlay = null;
                Console.WriteLine("[QRService] Yellow overlay removed.");
            }
            if (backButton != null)
            {
                backButton.Superview?.RemoveFromSuperview(); // Uklanja se čitav back bar
                backButton = null;
                Console.WriteLine("[QRService] Back button removed.");
            }
        }

        /// <summary>
        /// Ažurira detekciju QR koda i proverava njegovu stabilnost.
        /// Ako je isti kod detektovan minimum 300 ms, poziva se callback i skeniranje se zaustavlja.
        /// </summary>
        public void UpdateLastDetectedValue(string value)
        {
            if (string.IsNullOrEmpty(lastDetectedValue) || lastDetectedValue != value)
            {
                lastDetectedValue = value;
                stableStartTime = DateTime.Now;
                Console.WriteLine("[QRService] New QR detected: " + value);
            }
            else
            {
                if (stableStartTime.HasValue && (DateTime.Now - stableStartTime.Value) >= TimeSpan.FromMilliseconds(300))
                {
                    Console.WriteLine("[QRService] Stable QR code detected: " + value);
                    onScannedCallback?.Invoke(value);
                    StopScanning();
                }
            }
        }

        private UIView GetDefaultContainerView()
        {
            var keyWindow = UIApplication.SharedApplication.ConnectedScenes
                                .OfType<UIWindowScene>()
                                .FirstOrDefault()?.KeyWindow;
            if (keyWindow == null)
            {
                throw new InvalidOperationException("Key window not found.");
            }
            return keyWindow.RootViewController.View;
        }

        class QrMetadataOutputDelegate : AVCaptureMetadataOutputObjectsDelegate
        {
            Action<string> callback;
            AVCaptureSession session;
            QrScannerService_iOS service;

            public QrMetadataOutputDelegate(Action<string> callback, AVCaptureSession session, QrScannerService_iOS service)
            {
                this.callback = callback;
                this.session = session;
                this.service = service;
            }

            public override void DidOutputMetadataObjects(AVCaptureMetadataOutput output, AVMetadataObject[] metadataObjects, AVCaptureConnection connection)
            {
                if (metadataObjects != null && metadataObjects.Length > 0)
                {
                    foreach (var meta in metadataObjects)
                    {
                        if (meta is AVMetadataMachineReadableCodeObject codeObj)
                        {
                            if (!string.IsNullOrEmpty(codeObj.StringValue))
                            {
                                Console.WriteLine("[QRService] QR code detected (delegate): " + codeObj.StringValue);
                                service.UpdateLastDetectedValue(codeObj.StringValue);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif
