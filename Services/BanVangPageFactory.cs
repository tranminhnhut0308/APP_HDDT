using MyLoginApp.Pages;

namespace MyLoginApp.Services
{
    public class BanVangPageFactory : IBanVangPageFactory
    {
        private readonly IElectronicInvoiceService _electronicInvoiceService;

        public BanVangPageFactory(IElectronicInvoiceService electronicInvoiceService)
        {
            _electronicInvoiceService = electronicInvoiceService;
        }

        public BanVangPage Create()
        {
            return new BanVangPage(_electronicInvoiceService);
        }
    }
} 