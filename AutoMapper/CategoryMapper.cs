using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.AutoMapper
{
    public class CategoryMapper : Profile
    {
        public CategoryMapper()
        {
            CreateMap<CategoryVM, Category>();
            CreateMap<Category, CategoryVM>();
        }
    }
}