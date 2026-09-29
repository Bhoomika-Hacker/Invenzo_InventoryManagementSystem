using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.AutoMapper
{
    public class StockMovementMapper : Profile
    {
        public StockMovementMapper()
        {
            CreateMap<StockMovementVM, StockMovement>();
            CreateMap<StockMovement, StockMovementVM>();
        }
    }
}