using System.Collections.ObjectModel;
using MySqlConnector;
using MyLoginApp.Helpers;
using MyLoginApp.Models;
using MyLoginApp.Models.DanhMuc;
using static Microsoft.Maui.ApplicationModel.Permissions;
using System.Reflection;
using Plugin.Maui.Audio;
using ZXing.Common;
using SkiaSharp;
using ZXing;
using ZXing.QrCode;
using ZXing.SkiaSharp;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Text;
using Microsoft.Maui.Media;
using System.IO;
using ZXing.SkiaSharp;
using MyLoginApp.ViewModels;
using System.Globalization;

namespace MyLoginApp.Pages
{
    public partial class CamVangPage : ContentPage
    {
        private KhachHang? khachHangDaChon;
        private string? maCCCDDaQuet;
        private KhachHang? khachHangTuCCCD;
        private ObservableCollection<KhachHang> DanhSachKhachHang = new();
        private IAudioPlayer? _audioPlayer;
        private IAudioPlayer? _audioPlayerError;
        private CamVangViewModel _viewModel;

        public CamVangPage()
        {
            InitializeComponent();
            _viewModel = new CamVangViewModel();
            BindingContext = _viewModel;
            InitializeAudioPlayerAsync();

            // Thêm event handlers cho các entry
            entryCanTong.TextChanged += OnCanTongChanged;
            entryTLHot.TextChanged += OnTLHotChanged;
            entryTienCam.TextChanged += OnTienCamChanged;
            entryLaiSuat.TextChanged += OnLaiSuatChanged;

            // Thêm Completed event handlers để định dạng sau khi nhập
            entryCanTong.Completed += OnEntryCanTongCompleted;
            entryTLHot.Completed += OnEntryTLHotCompleted;
            entryTienCam.Completed += OnEntryTienCamCompleted;
            entryLaiSuat.Completed += OnEntryLaiSuatCompleted;
        }

        private void OnCanTongChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
        {
            if (e.NewTextValue == null) return;
            string cleanedText = e.NewTextValue.Replace(" gram", "").Trim();
            if (string.IsNullOrWhiteSpace(cleanedText)) return;
            if (double.TryParse(cleanedText, NumberStyles.Any, CultureInfo.CurrentCulture, out double canTong))
            {
                _viewModel.CanTong = canTong;
            }
            else if (string.IsNullOrWhiteSpace(cleanedText))
            {
                _viewModel.CanTong = null;
            }
        }

        private void OnTLHotChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
        {
            if (e.NewTextValue == null) return;
            string cleanedText = e.NewTextValue.Replace(" gram", "").Trim();
            if (string.IsNullOrWhiteSpace(cleanedText)) return;
            if (double.TryParse(cleanedText, NumberStyles.Any, CultureInfo.CurrentCulture, out double tlHot))
            {
                _viewModel.TlHot = tlHot;
            }
            else if (string.IsNullOrWhiteSpace(cleanedText))
            {
                _viewModel.TlHot = null;
            }
        }

        private void OnTienCamChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
        {
            if (e.NewTextValue == null) return;
            string cleanedText = e.NewTextValue.Replace(" VNĐ", "").Replace(".", "").Trim();
            if (string.IsNullOrWhiteSpace(cleanedText)) return;
            if (double.TryParse(cleanedText, NumberStyles.Any, CultureInfo.CurrentCulture, out double tienCam))
            {
                _viewModel.TienCam = tienCam;
            }
            else if (string.IsNullOrWhiteSpace(cleanedText))
            {
                _viewModel.TienCam = null;
            }
        }

        private void OnLaiSuatChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
        {
            if (e.NewTextValue == null) return;
            string cleanedText = e.NewTextValue.Replace(" %", "").Replace(",", ".").Trim();
            if (string.IsNullOrWhiteSpace(cleanedText)) return;
            if (double.TryParse(cleanedText, NumberStyles.Any, CultureInfo.InvariantCulture, out double laiSuat))
            {
                _viewModel.LaiSuat = laiSuat;
            }
            else if (string.IsNullOrWhiteSpace(cleanedText))
            {
                _viewModel.LaiSuat = null;
            }
        }

        private void OnEntryCanTongCompleted(object sender, EventArgs e)
        {
            if (sender is Entry entry)
            {
                // Không format lại, chỉ giữ nguyên giá trị người dùng nhập
                // string cleanedText = entry.Text?.Replace(" gram", "").Trim() ?? "";
                // if (double.TryParse(cleanedText, out double value))
                // {
                //     entry.Text = $"{value:N3}";
                // }
                // else if (string.IsNullOrWhiteSpace(cleanedText))
                // {
                //     entry.Text = "";
                // }
            }
        }

        private void OnEntryTLHotCompleted(object sender, EventArgs e)
        {
            if (sender is Entry entry)
            {
                // Không format lại, chỉ giữ nguyên giá trị người dùng nhập
                // string cleanedText = entry.Text?.Replace(" gram", "").Trim() ?? "";
                // if (double.TryParse(cleanedText, out double value))
                // {
                //     entry.Text = $"{value:N3}";
                // }
                // else if (string.IsNullOrWhiteSpace(cleanedText))
                // {
                //     entry.Text = "";
                // }
            }
        }

        private void OnEntryTienCamCompleted(object sender, EventArgs e)
        {
            if (sender is Entry entry)
            {
                string cleanedText = entry.Text?.Replace(" VNĐ", "").Replace(".", "").Trim() ?? "";
                if (double.TryParse(cleanedText, NumberStyles.Any, CultureInfo.CurrentCulture, out double value))
                {
                    entry.Text = $"{value:N0} VNĐ";
                }
                else if (string.IsNullOrWhiteSpace(cleanedText))
                {
                    entry.Text = "";
                }
            }
        }

        private void OnEntryLaiSuatCompleted(object sender, EventArgs e)
        {
            if (sender is Entry entry)
            {
                string cleanedText = entry.Text?.Replace(" %", "").Replace(",", ".").Trim() ?? "";
                if (double.TryParse(cleanedText, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                {
                    entry.Text = $"{value:N2} %";
                }
                else if (string.IsNullOrWhiteSpace(cleanedText))
                {
                    entry.Text = "";
                }
            }
        }

        private async void InitializeAudioPlayerAsync()
        {
            try
            {
                var audioService = AudioManager.Current;
                var stream = await FileSystem.OpenAppPackageFileAsync("Resources/Raw/beep.wav");
                _audioPlayer = audioService.CreatePlayer(stream);
                var streamError = await FileSystem.OpenAppPackageFileAsync("Resources/Raw/error.mp3");
                _audioPlayerError = audioService.CreatePlayer(streamError);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khởi tạo AudioPlayer: {ex.Message}");
            }
        }

        private async Task<string?> ChupVaQuetQRAsync()
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
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể đọc ảnh vừa chụp.", "OK");
                    return null;
                }

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
                        PossibleFormats = new List<ZXing.BarcodeFormat>
                        {
                            ZXing.BarcodeFormat.QR_CODE,
                            ZXing.BarcodeFormat.CODE_128,
                            ZXing.BarcodeFormat.CODE_39,
                            ZXing.BarcodeFormat.CODABAR
                        }
                    }
                };

                var result = reader.Decode(bitmap);
                if (result == null || string.IsNullOrWhiteSpace(result.Text))
                {
                    await Shell.Current.DisplayAlert("Thông báo", "Không tìm thấy mã. Vui lòng chụp mã rõ nét, chính diện và đủ sáng.", "OK");
                    return null;
                }
                if (_audioPlayer != null)
                {
                    _audioPlayer.Play();
                }
                return result.Text;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Có lỗi khi quét mã: {ex.Message}", "OK");
                return null;
            }
        }

        private async void OnChonKhachHangClicked(object sender, EventArgs e)
        {
            frameCustomerSelectionArea.IsVisible = true;
            frameQuetCCCD.IsVisible = false;
            await LoadKhachHangAsync();
        }

        private void OnTenKhachHangChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                lblThongTinKH.IsVisible = false;
                btnXacNhanKhach.IsVisible = false;
                return;
            }

            var khachHangTimThay = DanhSachKhachHang.FirstOrDefault(kh => 
                kh.TenKH.ToLower().Contains(e.NewTextValue.ToLower()));

            if (khachHangTimThay != null)
            {
                lblThongTinKH.Text = $"Tìm thấy: {khachHangTimThay.TenKH}\n" +
                                    $"SĐT: {khachHangTimThay.SoDienThoai}\n" +
                                    $"Địa chỉ: {khachHangTimThay.DiaChi}";
                lblThongTinKH.IsVisible = true;
                btnXacNhanKhach.IsVisible = true;
                khachHangDaChon = khachHangTimThay;
            }
            else
            {
                lblThongTinKH.Text = "❌ Không tìm thấy khách hàng. Vui lòng thêm mới.";
                lblThongTinKH.IsVisible = true;
                btnXacNhanKhach.IsVisible = false;
                frameThemKhach.IsVisible = true;
            }
        }

        private async void OnXacNhanKhachClicked(object sender, EventArgs e)
        {
            if (khachHangDaChon != null)
            {
                _viewModel.TenKhach = khachHangDaChon.TenKH;
                _viewModel.SoDienThoai = khachHangDaChon.SoDienThoai;
                _viewModel.KhachHangId = khachHangDaChon.MaKH;
                lblKhachHangDaChon.Text = $"👤 Khách hàng: {khachHangDaChon.TenKH}";
                lblKhachHangDaChon.IsVisible = true;
                frameCustomerSelectionArea.IsVisible = false;
                frameThemKhach.IsVisible = false;
                frameQuetCCCD.IsVisible = false;
            }
        }

        private async void OnThemKhachHangClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(entrySoDienThoai.Text) ||
                string.IsNullOrWhiteSpace(entryDiaChi.Text) ||
                string.IsNullOrWhiteSpace(entryCCCD.Text))
            {
                await DisplayAlert("Thông báo", "Vui lòng nhập đầy đủ thông tin khách hàng.", "OK");
                return;
            }

            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM phx_khach_hang WHERE CMND = @CMND", conn);
                checkCmd.Parameters.AddWithValue("@CMND", entryCCCD.Text);
                var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
                if (count > 0)
                {
                    await DisplayAlert("Thông báo", "CMND này đã tồn tại trong hệ thống.", "OK");
                    return;
                }

                var insertCmd = new MySqlCommand(@"
                    INSERT INTO phx_khach_hang (KH_TEN, DIEN_THOAI, DIA_CHI, CMND)
                    VALUES (@TenKH, @SoDienThoai, @DiaChi, @CMND);
                    SELECT LAST_INSERT_ID();", conn);

                insertCmd.Parameters.AddWithValue("@TenKH", entryTenKhach.Text);
                insertCmd.Parameters.AddWithValue("@SoDienThoai", entrySoDienThoai.Text);
                insertCmd.Parameters.AddWithValue("@DiaChi", entryDiaChi.Text);
                insertCmd.Parameters.AddWithValue("@CMND", entryCCCD.Text);

                var newId = await insertCmd.ExecuteScalarAsync();
                if (newId != null)
                {
                    khachHangDaChon = new KhachHang
                    {
                        MaKH = newId.ToString(),
                        TenKH = entryTenKhach.Text,
                        SoDienThoai = entrySoDienThoai.Text,
                        DiaChi = entryDiaChi.Text,
                        CMND = entryCCCD.Text
                    };

                    lblKhachHangDaChon.Text = $"👤 Khách hàng: {khachHangDaChon.TenKH}\n" +
                                             $"📞 SĐT: {khachHangDaChon.SoDienThoai}";
                    lblKhachHangDaChon.IsVisible = true;
                    frameCustomerSelectionArea.IsVisible = false;
                    frameThemKhach.IsVisible = false;
                    frameQuetCCCD.IsVisible = false;

                    entryTenKhach.Text = "";
                    entrySoDienThoai.Text = "";
                    entryDiaChi.Text = "";
                    entryCCCD.Text = "";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể thêm khách hàng: {ex.Message}", "OK");
            }
        }

        private async Task LoadKhachHangAsync()
        {
            try
            {
                using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                if (conn == null) return;

                var cmd = new MySqlCommand("SELECT * FROM phx_khach_hang ORDER BY KH_TEN", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                DanhSachKhachHang.Clear();
                while (await reader.ReadAsync())
                {
                    DanhSachKhachHang.Add(new KhachHang
                    {
                        MaKH = reader["KH_MA"].ToString(),
                        TenKH = reader["KH_TEN"].ToString(),
                        SoDienThoai = reader["DIEN_THOAI"].ToString(),
                        DiaChi = reader["DIA_CHI"].ToString(),
                        CMND = reader["CMND"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tải danh sách khách hàng: {ex.Message}", "OK");
            }
        }

        private async void OnQuetCCCDClicked(object sender, EventArgs e)
        {
            if (!await RequestCameraPermission())
            {
                await DisplayAlert("Thông báo", "Cần quyền truy cập camera để chụp ảnh CCCD.", "OK");
                return;
            }

            frameCustomerSelectionArea.IsVisible = true;
            frameQuetCCCD.IsVisible = true;

            var cccdResult = await ChupVaQuetQRAsync();

            if (!string.IsNullOrEmpty(cccdResult))
            {
                // PHÁT ÂM THANH BÍP khi quét CCCD thành công
                if (_audioPlayer != null)
                {
                    _audioPlayer.Play();
                }
                maCCCDDaQuet = cccdResult;
                try
                {
                    // Phân tích chuỗi CCCD
                    var cccdParts = cccdResult.Split('|');
                    if (cccdParts.Length >= 6)
                    {
                        string cccdNumber = cccdParts[0];
                        string hoTen = cccdParts[2];
                        string ngaySinh = cccdParts[3];
                        string gioiTinh = cccdParts[4];
                        string diaChi = cccdParts[5];

                        using var conn = await DatabaseHelper.GetOpenConnectionAsync();
                        if (conn == null) return;

                        var cmd = new MySqlCommand("SELECT * FROM phx_khach_hang WHERE CMND = @CMND", conn);
                        cmd.Parameters.AddWithValue("@CMND", cccdNumber);

                        using var reader = await cmd.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            khachHangTuCCCD = new KhachHang
                            {
                                MaKH = reader["KH_MA"].ToString(),
                                TenKH = reader["KH_TEN"].ToString(),
                                SoDienThoai = reader["DIEN_THOAI"].ToString(),
                                DiaChi = reader["DIA_CHI"].ToString(),
                                CMND = reader["CMND"].ToString()
                            };

                            lblCCCDInfo.Text = $"Tìm thấy khách hàng:\n" +
                                             $"Tên: {khachHangTuCCCD.TenKH}\n" +
                                             $"SĐT: {khachHangTuCCCD.SoDienThoai}\n" +
                                             $"Địa chỉ: {khachHangTuCCCD.DiaChi}";
                            lblCCCDInfo.IsVisible = true;
                            btnXacNhanCCCD.IsVisible = true;
                        }
                        else
                        {
                            // Nếu không tìm thấy trong DB, hiển thị form thêm mới với dữ liệu đã quét
                            lblCCCDInfo.Text = $"CCCD: {cccdNumber}\nKhông tìm thấy trong hệ thống. Vui lòng thêm mới.";
                            lblCCCDInfo.IsVisible = true;
                            frameThemKhach.IsVisible = true;
                            
                            // Điền dữ liệu vào form thêm mới
                            entryTenKhach.Text = hoTen;
                            entryCCCD.Text = cccdNumber;
                            entryDiaChi.Text = diaChi;
                            
                            // Hiển thị thông tin chi tiết
                            lblCCCDInfo.Text = $"Thông tin CCCD:\n" +
                                             $"Họ tên: {hoTen}\n" +
                                             $"Ngày sinh: {ngaySinh}\n" +
                                             $"Giới tính: {gioiTinh}\n" +
                                             $"Địa chỉ: {diaChi}";
                        }
                    }
                    else
                    {
                        if (_audioPlayerError != null)
                        {
                            _audioPlayerError.Play();
                        }
                        await DisplayAlert("Lỗi", "Định dạng CCCD không hợp lệ", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Lỗi", $"Không thể xử lý kết quả quét: {ex.Message}", "OK");
                }
            }
            else
            {
                if (_audioPlayerError != null)
                {
                    _audioPlayerError.Play();
                }
                lblCCCDInfo.Text = "Không có mã CCCD nào được phát hiện.";
                lblCCCDInfo.IsVisible = true;
            }
        }

        private async Task<bool> RequestCameraPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status == PermissionStatus.Granted)
                return true;

            status = await Permissions.RequestAsync<Permissions.Camera>();
            return status == PermissionStatus.Granted;
        }

        private async void OnXacNhanCCCDClicked(object sender, EventArgs e)
        {
            if (khachHangTuCCCD != null)
            {
                _viewModel.TenKhach = khachHangTuCCCD.TenKH;
                _viewModel.SoDienThoai = khachHangTuCCCD.SoDienThoai;
                lblKhachHangDaChon.Text = $"👤 Khách hàng: {khachHangTuCCCD.TenKH}";
                lblKhachHangDaChon.IsVisible = true;
                frameQuetCCCD.IsVisible = false;
            }
        }

        private async void OnThanhToanClicked(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem ViewModel có được khởi tạo không
                if (_viewModel == null)
                {
                    await DisplayAlert("Lỗi", "Không thể khởi tạo ViewModel!", "OK");
                    return;
                }

                // Gọi phương thức thanh toán từ ViewModel và lấy mã phiếu vừa tạo
                var maPhieu = await _viewModel.OnThanhToanClickedAsync();
                // Sau khi tạo phiếu thành công, reset các Entry (nếu có mã phiếu)
                if (!string.IsNullOrEmpty(maPhieu))
                {
                    ResetAllEntries();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        private void ResetAllEntries()
        {
            try
            {
                if (entryTenHang != null) entryTenHang.Text = "";
                if (entryCanTong != null) entryCanTong.Text = "";
                if (entryTLHot != null) entryTLHot.Text = "";
                if (entryTienCam != null) entryTienCam.Text = "";
                if (entryLaiSuat != null) entryLaiSuat.Text = "";
                if (entryGhiChu != null) entryGhiChu.Text = "";
                // Nếu có các Entry khác cần reset, thêm vào đây
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ResetAllEntries] Lỗi: {ex.Message}");
            }
        }
    }
}
