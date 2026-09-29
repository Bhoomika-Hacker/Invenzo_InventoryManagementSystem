using InventoryManagementSystem.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Invenzo.Tests.Controllers;

public class PurchaseControllerTests
{
    private static Type ControllerType => typeof(PurchaseController);

    [Fact] public void PurchaseController_Exists() => Assert.NotNull(ControllerType);
    [Fact] public void PurchaseController_IsPublic() => Assert.True(ControllerType.IsPublic);
    [Fact] public void PurchaseController_InheritsMvcController() => Assert.True(typeof(Controller).IsAssignableFrom(ControllerType));
    [Fact] public void PurchaseController_HasIndexAction() => Assert.NotNull(ControllerType.GetMethod("Index"));
    [Fact] public void PurchaseController_HasRequiredAuthorization() => Assert.Contains(ControllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true), a => ((AuthorizeAttribute)a).Roles == "Admin,Seller");
}
