using System.Collections.Generic;

namespace MyLoginApp.Services
{
    public interface IElectronicInvoiceService
    {
        Task<bool> CreateElectronicInvoiceAsync(
            List<InvoiceItem> items,
            decimal totalAmount,
            decimal discountAmount,
            string customerId,
            string customerName,
            string customerAddress,
            string customerPhone,
            string customerCMND,
            string orderCode);
    }

    public class InvoiceItem
    {
        public string ItemName { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ItemTotalAmountWithoutTax { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ItemTotalAmountWithTax { get; set; }
    }
} 