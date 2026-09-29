using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.AutoMapper
{
    public class PurchaseMapper : Profile
    {
        public PurchaseMapper()
        {
            CreateMap<PurchaseVM, Purchase>();
            CreateMap<Purchase, PurchaseVM>();
        }
    }
}