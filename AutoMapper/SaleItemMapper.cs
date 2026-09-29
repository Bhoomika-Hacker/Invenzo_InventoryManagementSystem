using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.AutoMapper
{
    public class SaleItemMapper : Profile
    {
        public SaleItemMapper()
        {
            CreateMap<SaleItemVM, SaleItem>();
            CreateMap<SaleItem, SaleItemVM>();
        }
    }
}