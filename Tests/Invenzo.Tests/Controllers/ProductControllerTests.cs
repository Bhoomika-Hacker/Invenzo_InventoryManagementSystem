using InventoryManagementSystem.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Invenzo.Tests.Controllers;

public class ProductControllerTests
{
    private static Type ControllerType => typeof(ProductController);

    [Fact] public void ProductController_Exists() => Assert.NotNull(ControllerType);
    [Fact] public void ProductController_IsPublic() => Assert.True(ControllerType.IsPublic);
    [Fact] public void ProductController_InheritsMvcController() => Assert.True(typeof(Controller).IsAssignableFrom(ControllerType));
    [Fact] public void ProductController_HasIndexAction() => Assert.NotNull(ControllerType.GetMethod("Index"));
    [Fact] public void ProductController_HasRequiredAuthorization() => Assert.Contains(ControllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true), a => ((AuthorizeAttribute)a).Roles == "Admin,Seller");
}
