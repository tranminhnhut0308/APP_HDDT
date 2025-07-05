# Hướng dẫn tích hợp Hóa đơn điện tử vào BanVangPage

## Tổng quan
Đã tích hợp thành công hóa đơn điện tử vào trang BanVangPage. Khi người dùng nhấn nút "Thanh toán", hệ thống sẽ:
1. Tạo phiếu xuất trong database
2. Tự động tạo hóa đơn điện tử qua API hoadon30s.vn
3. Hiển thị thông báo thành công/thất bại

## Các file đã thêm/sửa đổi

### 1. Services (Mới)
- `Services/IElectronicInvoiceService.cs` - Interface cho service hóa đơn điện tử
- `Services/ElectronicInvoiceService.cs` - Implementation của service
- `Services/IBanVangPageFactory.cs` - Interface factory cho BanVangPage
- `Services/BanVangPageFactory.cs` - Implementation factory
- `Services/BanVangPageRouteHandler.cs` - Custom route handler

### 2. Pages (Sửa đổi)
- `Pages/GiaoDich/BanVangPage.xaml.cs` - Thêm tích hợp hóa đơn điện tử

### 3. Configuration (Sửa đổi)
- `MauiProgram.cs` - Đăng ký services và dependency injection
- `App.xaml.cs` - Cập nhật để sử dụng DI
- `AppShell.xaml.cs` - Cập nhật route handler
- `MyLoginApp.csproj` - Thêm package Newtonsoft.Json

## Cách hoạt động

### 1. Dependency Injection
- `IElectronicInvoiceService` được đăng ký như singleton
- `BanVangPage` được tạo thông qua factory pattern
- Custom route handler đảm bảo DI hoạt động với Shell navigation

### 2. Quy trình thanh toán
```csharp
// Trong OnThanhToanClicked
bool phieuXuatCreated = await TaoPhieuXuat(khachHangDaChon.MaKH, scannedItems);

if (phieuXuatCreated)
{
    // Tạo hóa đơn điện tử
    bool electronicInvoiceCreated = await CreateElectronicInvoiceAsync();

    if (electronicInvoiceCreated)
    {
        await DisplayAlert("Thành công", "✅ Hóa đơn điện tử đã được tạo thành công!", "OK");
    }
    else
    {
        await DisplayAlert("Cảnh báo", "⚠️ Hóa đơn điện tử tạo thất bại, nhưng phiếu xuất đã được lưu.", "OK");
    }
}
```

### 3. Cấu trúc dữ liệu hóa đơn
- **Mặt hàng vàng**: Đơn vị "Chỉ", giá trị = trọng lượng/100
- **Tiền công**: Đơn vị "Cái", tách riêng cho từng mặt hàng
- **Thuế**: Mặc định 0% cho vàng
- **Tổng tiền**: Bao gồm cả tiền công

## API Configuration
- **Base URL**: https://cpanel.hoadon30s.vn
- **Client ID**: 2b12baad-c037-46a3-b953-2629fc759032
- **Client Secret**: d9845a8ee363d447ef704a9d61f0076b02ffb151
- **Scope**: create-invoice

## Xử lý lỗi
- Kiểm tra kết nối API trước khi gửi request
- Xử lý lỗi authentication
- Thông báo rõ ràng cho người dùng
- Phiếu xuất vẫn được lưu ngay cả khi hóa đơn điện tử thất bại

## Testing
1. Chạy ứng dụng
2. Đi đến trang Bán Vàng
3. Chọn khách hàng
4. Quét mã vàng
5. Nhấn "Thanh toán"
6. Kiểm tra thông báo hóa đơn điện tử

## Lưu ý
- Cần có kết nối internet để tạo hóa đơn điện tử
- Thông tin khách hàng phải đầy đủ (tên, địa chỉ, số điện thoại)
- Hóa đơn điện tử được tạo tự động, không cần xác nhận thêm 