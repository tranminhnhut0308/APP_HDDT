using MyLoginApp.ViewModels;
using Microsoft.Maui.Controls;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.IO;

namespace MyLoginApp.Pages;

public partial class KhoVangCamPage : ContentPage
{
    private KhoVangCamViewModel _viewModel;

    public KhoVangCamPage()
    {
        InitializeComponent();
        _viewModel = new KhoVangCamViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            await _viewModel.LoadKhoVangCamAsync();
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SearchKeyword = e.NewTextValue;
            await _viewModel.LoadKhoVangCamAsync(_viewModel.SearchKeyword);
        }
    }

    private async void OnTimKiemClicked(object sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            string action = await DisplayActionSheet("Chọn cách tìm kiếm", "Huỷ", null, "Nhập mã phiếu", "Quét mã QR");
            if (action == "Nhập mã phiếu")
            {
                string keyword = await DisplayPromptAsync("Tìm kiếm", "Nhập mã phiếu hoặc tên khách hàng:", "Tìm", "Hủy", initialValue: _viewModel.SearchKeyword);
                if (keyword != null)
                {
                    _viewModel.SearchKeyword = keyword;
                    await _viewModel.LoadKhoVangCamAsync(keyword);
                }
            }
            else if (action == "Quét mã QR")
            {
                var result = await ChupVaQuetQRAsync();
                if (!string.IsNullOrWhiteSpace(result))
                {
                    _viewModel.SearchKeyword = result;
                    await _viewModel.LoadKhoVangCamAsync(result);
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
}
