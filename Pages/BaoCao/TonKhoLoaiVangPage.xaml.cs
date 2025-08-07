using MyLoginApp.ViewModels.BaoCao;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;
using System.Threading.Tasks;

namespace MyLoginApp.Pages.BaoCao;

public partial class TonKhoLoaiVangPage : ContentPage
{
	public TonKhoLoaiVangPage()
	{
		InitializeComponent();
	}

	private async void OnTimKiemClicked(object sender, EventArgs e)
	{
		var viewModel = BindingContext as TonKhoLoaiVangViewModel;
		if (viewModel == null) return;

		string action = await DisplayActionSheet("Chọn cách tìm kiếm", "Huỷ", null, "Nhập từ khóa", "Quét mã QR");
		if (action == "Nhập từ khóa")
		{
			string tuKhoa = await DisplayPromptAsync("Tìm kiếm", "Nhập từ khóa:", "OK", "Hủy", viewModel.TuKhoaTimKiem);
			if (tuKhoa != null)
			{
				viewModel.TuKhoaTimKiem = tuKhoa;
				if (viewModel.ThucHienTimKiemCommand.CanExecute(null))
				{
					viewModel.ThucHienTimKiemCommand.Execute(null);
				}
			}
		}
		else if (action == "Quét mã QR")
		{
			var result = await ChupVaQuetQRAsync();
			if (!string.IsNullOrWhiteSpace(result))
			{
				viewModel.TuKhoaTimKiem = result;
				if (viewModel.ThucHienTimKiemCommand.CanExecute(null))
				{
					viewModel.ThucHienTimKiemCommand.Execute(null);
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