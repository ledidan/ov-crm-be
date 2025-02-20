using System.Collections.Generic;
using Data.DTOs;
using Data.MongoModels;


namespace Data.DTOs
{
    public class CreateInvoiceDTO
    {
        public required InvoiceDTO Invoice { get; set; }    
        // public List<OrderDetailDTO> OrderDetails { get; set; } = new();
    }
}