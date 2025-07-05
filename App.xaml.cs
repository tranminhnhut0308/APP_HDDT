using MyLoginApp.Helpers;
using Application = Microsoft.Maui.Controls.Application;
using MyLoginApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MyLoginApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent(); // Load UI từ App.xaml

        DatabaseHelper.LoadSavedConnectionString(); // Thêm dòng này để tải chuỗi kết nối đã lưu

        // Sử dụng dependency injection để tạo AppShell
        var services = MauiProgram.Services;
        var appShell = services.GetService<AppShell>();
        MainPage = appShell ?? new AppShell();
    }
}
