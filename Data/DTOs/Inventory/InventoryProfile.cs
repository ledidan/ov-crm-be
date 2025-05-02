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
            CreateMap<CreateInventoryDTO, ProductInventory>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.QuantityInStock, opt => opt.MapFrom(src => src.QuantityInStock))
            .ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => src.SupplierId))
            .ForMember(dest => dest.InventoryCode, opt => opt.MapFrom(src => src.InventoryCode))
            .ForMember(dest => dest.UnitOfMeasure, opt => opt.MapFrom(src => src.UnitOfMeasure))
            .ForMember(dest => dest.DateReceived, opt => opt.MapFrom(src => src.DateReceived))
            .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.BatchNumber))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.WarehouseLocation, opt => opt.MapFrom(src => src.WarehouseLocation))
            .ForMember(dest => dest.OrderQuantity, opt => opt.MapFrom(src => src.OrderQuantity))
            .ForMember(dest => dest.MinimumStockLevel, opt => opt.MapFrom(src => src.MinimumStockLevel))
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.AvailableQuantity))
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
            .ForMember(dest => dest.ReturnedQuantity, opt => opt.MapFrom(src => src.ReturnedQuantity))
            .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src => src.StockStatus));
            // Ánh xạ từ UpdateInventoryDTO sang ProductInventory
            CreateMap<UpdateInventoryDTO, ProductInventory>();

            // Ánh xạ từ ProductInventory sang InventoryDTO (response)
            CreateMap<ProductInventory, InventoryDTO>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.SupplierName : null))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.Product != null && src.Product.Partner != null ? src.Product.Partner.Id : 0));
        }


    }
}