using MyLoginApp.ViewModels.BaoCao;
using ZXing.Common;
using SkiaSharp;
using ZXing;
using ZXing.QrCode;
using ZXing.SkiaSharp;

namespace MyLoginApp.Pages.BaoCao;

public partial class PhieuXuatPage : ContentPage
{
    private PhieuXuatViewModel _viewModel;

    public PhieuXuatPage()
    {
        InitializeComponent();
        _viewModel = new PhieuXuatViewModel();
        BindingContext = _viewModel;

        // Debug thông tin ViewModel
        Console.WriteLine(_viewModel);
    }

    private async void OnTimKiemClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Tìm kiếm phiếu xuất", "Hủy", null, "Nhập mã phiếu", "Quét mã");
        if (action == "Nhập mã phiếu")
        {
            string keyword = await DisplayPromptAsync("Tìm kiếm", "Nhập mã phiếu hoặc tên hàng hóa:", "Tìm", "Hủy");
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                _viewModel.SearchKeyword = keyword;
            }
        }
        else if (action == "Quét mã")
        {
            string result = await ChupVaQuetQRAsync();
            if (!string.IsNullOrWhiteSpace(result))
            {
                _viewModel.SearchKeyword = result;
            }
        }
    }

    private async void OnSearchTextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SearchKeyword = e.NewTextValue;
        }
    }

    private async void OnQuetMaClicked(object sender, EventArgs e)
    {
        string result = await ChupVaQuetQRAsync();
        if (!string.IsNullOrWhiteSpace(result))
        {
            _viewModel.SearchKeyword = result;
        }
    }

    // Hàm quét mã QR/barcode thực tế (chụp và giải mã)
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

            // Cấu hình BarcodeReader tối ưu
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Console.WriteLine("OnAppearing called");  // Debug log
        if (_viewModel != null)
        {
            await _viewModel.LoadPhieuXuatAsync();
        }
    }

}
