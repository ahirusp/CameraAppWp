using System;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

            //ハードウェアキーのイベント登録
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

        /// <summary>
        /// 戻るを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            releaseCapture();
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        private async void releaseCapture()
        {
            if (isPreviewing)
            {
                captureElement.Source = null;
                await mediaCapture.StopPreviewAsync();
                mediaCapture.Dispose();
                isPreviewing = false;
            }
        }

        /// <summary>
        /// 画面サイズ（端末の向き）変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            border.Width = this.ActualHeight;
            border.Height = this.ActualHeight;
            captureElement.Width = this.ActualHeight;
            captureElement.Height = this.ActualWidth;
        }

        /// <summary>
        /// シャッターボタンを押した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void HardwareButtons_CameraPressed(object sender, CameraEventArgs e)
        {
            StorageFolder cameraRollFolder = KnownFolders.CameraRoll;
            StorageFile file = await cameraRollFolder.CreateFileAsync("test.jpg", CreationCollisionOption.ReplaceExisting);
            await mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
        }

    }
}
