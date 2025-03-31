using AutoMapper;
using Data.DTOs;
using Data.Entities;

namespace Services.Mappers
{
    public class InventoryProfile : Profile
    {
        public InventoryProfile()
        {
            CreateMap<CreateInventoryDTO, ProductInventory>();

            // Ánh xạ từ UpdateInventoryDTO sang ProductInventory
            CreateMap<UpdateInventoryDTO, ProductInventory>();

            // Ánh xạ từ ProductInventory sang InventoryDTO (response)
            CreateMap<ProductInventory, InventoryDTO>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.SupplierName : null))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.Product != null && src.Product.Partner != null ? src.Product.Partner.Id : 0));
        }
    }
}