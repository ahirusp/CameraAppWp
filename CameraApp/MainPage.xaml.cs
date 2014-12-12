using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=391641 を参照してください

namespace CameraApp
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        private bool isPreviewing = false;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            HardwareButtons.CameraPressed += HardwareButtons_CameraPressed;
        }

        /// <summary>
        /// このページがフレームに表示されるときに呼び出されます。
        /// </summary>
        /// <param name="e">このページにどのように到達したかを説明するイベント データ。
        /// このプロパティは、通常、ページを構成するために使用します。</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            mediaCapture = new MediaCapture();

            await mediaCapture.InitializeAsync();
            captureElement.Source = mediaCapture;

            await mediaCapture.StartPreviewAsync();
            isPreviewing = true;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            releaseCapture();
        }

        private void releaseCapture()
        {
            if (isPreviewing)
            {
                captureElement.Source = null;
                mediaCapture.StopPreviewAsync();
                mediaCapture.Dispose();
                isPreviewing = false;
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            border.Width = this.ActualHeight;
            border.Height = this.ActualHeight;
            captureElement.Width = this.ActualHeight;
            captureElement.Height = this.ActualWidth;
        }

        private async void HardwareButtons_CameraPressed(object sender, CameraEventArgs e)
        {
            StorageFolder cameraRollFolder = KnownFolders.CameraRoll;
            StorageFile file = await cameraRollFolder.CreateFileAsync("test.jpg", CreationCollisionOption.ReplaceExisting);
            await mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
        }

    }
}
