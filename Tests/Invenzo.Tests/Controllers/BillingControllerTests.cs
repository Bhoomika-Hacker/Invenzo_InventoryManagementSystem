using InventoryManagementSystem.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Invenzo.Tests.Controllers;

public class BillingControllerTests
{
    private static Type ControllerType => typeof(BillingController);

    [Fact] public void BillingController_Exists() => Assert.NotNull(ControllerType);
    [Fact] public void BillingController_IsPublic() => Assert.True(ControllerType.IsPublic);
    [Fact] public void BillingController_InheritsMvcController() => Assert.True(typeof(Controller).IsAssignableFrom(ControllerType));
    [Fact] public void BillingController_HasIndexAction() => Assert.NotNull(ControllerType.GetMethod("Index"));
    [Fact] public void BillingController_HasRequiredAuthorization() => Assert.Contains(ControllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true), a => ((AuthorizeAttribute)a).Roles == "Admin,Seller");
}
