using Microsoft.Maui.Controls;
using MyLoginApp.ViewModels;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.IO;

namespace MyLoginApp.Pages
{
    public partial class PhieuQuaHanPage : ContentPage
    {
        private PhieuQuaHanViewModel viewModel;

        public PhieuQuaHanPage()
        {
            InitializeComponent();
            viewModel = new PhieuQuaHanViewModel();
            BindingContext = viewModel;

            // Gọi load dữ liệu khi trang được tạo
            Loaded += async (s, e) => await viewModel.LoadPhieuQuaHanAsync();
        }

        private async void OnTimKiemClicked(object sender, EventArgs e)
        {
            if (viewModel != null)
            {
                string action = await DisplayActionSheet("Chọn cách tìm kiếm", "Huỷ", null, "Nhập mã phiếu", "Quét mã QR");
                if (action == "Nhập mã phiếu")
                {
                    string keyword = await DisplayPromptAsync("Tìm kiếm", "Nhập mã phiếu hoặc tên khách hàng:", "Tìm", "Hủy", initialValue: viewModel.SearchKeyword);
                    if (keyword != null)
                    {
                        viewModel.SearchKeyword = keyword;
                    }
                }
                else if (action == "Quét mã QR")
                {
                    var result = await ChupVaQuetQRAsync();
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        viewModel.SearchKeyword = result;
                    }
                }
            }
        }

        private async Task<string> ChupVaQuetQRAsync()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo == null)
                    return null;

                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                var bitmap = SKBitmap.Decode(imageBytes);
                if (bitmap == null)
                {
                    await DisplayAlert("Lỗi", "Không thể đọc ảnh vừa chụp.", "OK");
                    return null;
                }

                // Resize ảnh nếu quá lớn để đảm bảo quét chính xác
                const int maxWidth = 1024;
                if (bitmap.Width > maxWidth)
                {
                    float scale = (float)maxWidth / bitmap.Width;
                    var resized = bitmap.Resize(
                        new SKImageInfo((int)(bitmap.Width * scale), (int)(bitmap.Height * scale)),
                        SKFilterQuality.High);
                    bitmap.Dispose();
                    bitmap = resized;
                }

                var reader = new BarcodeReader<SKBitmap>(bmp => new SKBitmapLuminanceSource(bmp))
                {
                    AutoRotate = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        PureBarcode = false,
                        PossibleFormats = new List<BarcodeFormat>
                        {
                            BarcodeFormat.QR_CODE,
                            BarcodeFormat.CODE_128,
                            BarcodeFormat.CODE_39,
                            BarcodeFormat.CODABAR,
                            BarcodeFormat.EAN_13,
                            BarcodeFormat.EAN_8,
                            BarcodeFormat.UPC_A,
                            BarcodeFormat.UPC_E
                        }
                    }
                };

                var result = reader.Decode(bitmap);
                if (result == null || string.IsNullOrWhiteSpace(result.Text))
                {
                    await DisplayAlert("Thông báo", "Không tìm thấy mã. Vui lòng chụp mã rõ nét, chính diện và đủ sáng.", "OK");
                    return null;
                }

                return result.Text;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Có lỗi khi quét mã: {ex.Message}", "OK");
                return null;
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.SearchKeyword = e.NewTextValue;
                // Không cần gọi LoadPhieuQuaHanAsync() ở đây vì OnSearchKeywordChanged trong ViewModel sẽ tự động gọi
            }
        }

    }
}
