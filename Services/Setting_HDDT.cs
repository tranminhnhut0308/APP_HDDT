using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace MyLoginApp.Services
{
    public class Setting_HDDT : IElectronicInvoiceService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://cpanel.hoadon30s.vn";
        private const string ClientId = "2b12baad-c037-46a3-b953-2629fc759032";
        private const string ClientSecret = "d9845a8ee363d447ef704a9d61f0076b02ffb151";

        public Setting_HDDT()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> CreateElectronicInvoiceAsync(
            List<InvoiceItem> items,
            decimal totalAmount,
            decimal discountAmount,
            string customerId,
            string customerName,
            string customerAddress,
            string customerPhone,
            string customerCMND,
            string orderCode)
        {
            try
            {
                string token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                if (!await CheckApiConnectionAsync())
                {
                    return false;
                }

                // Chuẩn bị danh sách chi tiết hàng hóa
                var detailItems = new List<object>();
                foreach (var item in items)
                {
                    decimal taxRate = item.TaxRate > 0 ? item.TaxRate : 0.1m;
                    decimal taxAmount = item.TaxAmount > 0 ? item.TaxAmount : (item.ItemTotalAmountWithoutTax * taxRate);
                    decimal totalWithTax = item.ItemTotalAmountWithoutTax + taxAmount;

                    detailItems.Add(new
                    {
                        num = (object)null,
                        name = item.ItemName,
                        code = "",
                        unit = item.UnitName,
                        quantity = item.Quantity,
                        price = item.UnitPrice,
                        total = 0,
                        detailTotal = item.ItemTotalAmountWithoutTax,
                        vatRate = (object)null,
                        detialVatRate = taxRate,
                        vatAmount = 0,
                        detailVatAmount = taxAmount,
                        amount = 0,
                        detailAmount = totalWithTax,
                        feature = 1
                    });
                }

                // Kiểm tra dữ liệu
                if (detailItems.Count == 0 || totalAmount == 0)
                {
                    return false;
                }

                // Tạo đối tượng JSON request
                var invoiceRequest = new
                {
                    serial = "2C25MTT",
                    init_invoice = "HDBHMTT",
                    action = "create",
                    date_export = DateTime.Now.ToString("yyyy-MM-dd"),
                    order_code = orderCode,
                    customer_code = customerId ?? "",
                    currency = "VND",
                    payment_type = "3",
                    customer = new
                    {
                        cus_name = "",
                        cus_buyer = customerName,
                        cus_tax_code = "",
                        cus_address = customerAddress ?? "",
                        cus_phone = customerPhone ?? "",
                        cus_email = "",
                        cus_email_cc = "",
                        cus_citizen_identity = customerCMND ?? "",
                        cus_bank_no = "",
                        cus_bank_name = ""
                    },
                    vat_amount = totalAmount * 0.1m,
                    total = totalAmount,
                    amount = totalAmount,
                    detail = detailItems
                };

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string jsonContent = JsonConvert.SerializeObject(invoiceRequest, Formatting.Indented);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/invoice/create-cash-register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Parse response
                dynamic responseObj = JsonConvert.DeserializeObject(responseContent);
                string status = responseObj?.status?.ToString();
                string message = responseObj?.message?.ToString();

                return response.IsSuccessStatusCode && status == "200";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo hóa đơn điện tử: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var values = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", ClientId },
                    { "client_secret", ClientSecret },
                    { "scope", "create-invoice" }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await _httpClient.PostAsync($"{BaseUrl}/oauth/token", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                dynamic obj = JsonConvert.DeserializeObject(responseString);
                return obj?.access_token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy token: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CheckApiConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
} 