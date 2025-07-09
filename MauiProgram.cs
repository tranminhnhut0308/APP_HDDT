using Camera.MAUI;
using CommunityToolkit.Maui;
using MyLoginApp.Converters;
using MyLoginApp.ViewModels;
using ZXing.Net.Maui.Controls;
using MyLoginApp.ViewModels.BaoCao;
using MyLoginApp.Pages.BaoCao;
using MyLoginApp.Services;

namespace MyLoginApp;

public static class MauiProgram
{
    public static IServiceProvider Services { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() // Nếu có dùng CommunityToolkit
            .UseMauiCameraView()      // Tích hợp Camera.MAUI
            .UseBarcodeReader()       // Thêm dòng này để đăng ký ZXing
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
            
        // Đăng ký các ViewModel
        builder.Services.AddSingleton<NhomVangViewModel>();
        builder.Services.AddSingleton<LoaiVangViewModel>();
        builder.Services.AddSingleton<HangHoaViewModel>();

        // Đăng ký Converters
        builder.Services.AddTransient<LaiLoToColorConverter>();

        // Đăng ký ViewModels
        builder.Services.AddSingleton<PhieuXuatViewModel>();

        // Đăng ký Pages
        builder.Services.AddSingleton<PhieuXuatPage>();

        // Đăng ký Services
        builder.Services.AddSingleton<IElectronicInvoiceService, Setting_HDDT>();
        builder.Services.AddSingleton<IBanVangPageFactory, BanVangPageFactory>();
        builder.Services.AddSingleton<AppShell>();

        var app = builder.Build();
        Services = app.Services;
        return app;
    }
}