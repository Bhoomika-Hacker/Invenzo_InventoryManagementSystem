using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InventoryManagementSystem.AutoMapper
{
    public class ProductMapper : Profile
    {
        public ProductMapper()
        {
            // Save: ViewModel → Model
            CreateMap<ProductVM, Product>();

            // Display/Edit: Model → ViewModel
            CreateMap<Product, ProductVM>();
        }
    }
}