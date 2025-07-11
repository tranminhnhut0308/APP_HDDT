﻿using Microsoft.Maui.Controls;
using MyLoginApp.ViewModels;

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
                string keyword = await DisplayPromptAsync("Tìm kiếm", "Nhập mã phiếu hoặc tên khách hàng:", "Tìm", "Hủy", initialValue: viewModel.SearchKeyword);
                if (keyword != null) // User clicked "Tìm" or "Hủy"
                {
                    viewModel.SearchKeyword = keyword; // Set keyword to trigger search in ViewModel
                }
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
