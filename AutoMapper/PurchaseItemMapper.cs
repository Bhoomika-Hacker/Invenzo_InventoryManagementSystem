using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.AutoMapper
{
    public class PurchaseItemMapper : Profile
    {
        public PurchaseItemMapper()
        {
            CreateMap<PurchaseItemVM, PurchaseItem>();
            CreateMap<PurchaseItem, PurchaseItemVM>();
        }
    }
}