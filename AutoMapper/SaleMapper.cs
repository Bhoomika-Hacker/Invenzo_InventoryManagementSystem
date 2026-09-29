using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.AutoMapper
{
    public class SaleMapper : Profile
    {
        public SaleMapper()
        {
            CreateMap<SaleVM, Sale>();
            CreateMap<Sale, SaleVM>();
        }
    }
}