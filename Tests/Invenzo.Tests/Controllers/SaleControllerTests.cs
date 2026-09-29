using InventoryManagementSystem.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Invenzo.Tests.Controllers;

public class SaleControllerTests
{
    private static Type ControllerType => typeof(SaleController);

    [Fact] public void SaleController_Exists() => Assert.NotNull(ControllerType);
    [Fact] public void SaleController_IsPublic() => Assert.True(ControllerType.IsPublic);
    [Fact] public void SaleController_InheritsMvcController() => Assert.True(typeof(Controller).IsAssignableFrom(ControllerType));
    [Fact] public void SaleController_HasIndexAction() => Assert.NotNull(ControllerType.GetMethod("Index"));
    [Fact] public void SaleController_HasRequiredAuthorization() => Assert.Contains(ControllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true), a => ((AuthorizeAttribute)a).Roles == "Admin,Seller");
}
