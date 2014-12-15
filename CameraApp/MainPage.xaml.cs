using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
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
            using (var randomAccessStream = new InMemoryRandomAccessStream())
            {
                await mediaCapture.CapturePhotoToStreamAsync(
                    ImageEncodingProperties.CreateJpeg(), randomAccessStream);

                var bmp = await GetWriteableBitmap(randomAccessStream);

                await EffectMonochrome(bmp);
                await SaveToCameraRoll(bmp);
            }
        }

        /// <summary>
        /// WriteableBitmapに変換
        /// </summary>
        /// <param name="randomAccessStream"></param>
        /// <returns></returns>
        private async Task<WriteableBitmap> GetWriteableBitmap(InMemoryRandomAccessStream randomAccessStream)
        {
            var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
            var transform = new BitmapTransform();
            var pixelData = await decoder.GetPixelDataAsync(
                decoder.BitmapPixelFormat, decoder.BitmapAlphaMode, transform,
                ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
            var pixels = pixelData.DetachPixelData();

            var width = (int) decoder.OrientedPixelWidth;
            var height = (int) decoder.OrientedPixelHeight;

            var bmp = new WriteableBitmap(width, height);

            using (var pixelStream = bmp.PixelBuffer.AsStream())
            {
                pixelStream.Seek(0, SeekOrigin.Begin);
                pixelStream.Write(pixels, 0, pixels.Length);
            }
            return bmp;
        }

        /// <summary>
        /// 白黒画像に変換
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private async Task EffectMonochrome(WriteableBitmap bmp)
        {
            var pixels = bmp.PixelBuffer.ToArray();
            for (var i = 0; i < pixels.Length; i+=4)
            {
                var r = pixels[i + 2];
                var g = pixels[i + 1];
                var b = pixels[i];

                var avg = (byte)((r + g + b) / 3.0);
                pixels[i] = avg;
                pixels[i + 1] = avg;
                pixels[i + 2] = avg;
            }

            using (var pixelStreamstream = bmp.PixelBuffer.AsStream())
            {
                await pixelStreamstream.WriteAsync(pixels, 0, pixels.Length);
            }
        }

        /// <summary>
        /// カメラロールへ保存
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private async Task SaveToCameraRoll(WriteableBitmap bmp)
        {
            StorageFolder cameraRollFolder = KnownFolders.CameraRoll;
            StorageFile file = await cameraRollFolder.CreateFileAsync(
                "test.jpg", CreationCollisionOption.ReplaceExisting);

            using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);
                using (var pixelStream = bmp.PixelBuffer.AsStream())
                {
                    var pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                        (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, 72.0, 72.0, pixels);
                    await encoder.FlushAsync();
                }
            }
        }
    }
}
