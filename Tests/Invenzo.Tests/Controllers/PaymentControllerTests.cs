using InventoryManagementSystem.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Invenzo.Tests.Controllers;

public class PaymentControllerTests
{
    private static Type ControllerType => typeof(PaymentController);

    [Fact] public void PaymentController_Exists() => Assert.NotNull(ControllerType);
    [Fact] public void PaymentController_IsPublic() => Assert.True(ControllerType.IsPublic);
    [Fact] public void PaymentController_InheritsMvcController() => Assert.True(typeof(Controller).IsAssignableFrom(ControllerType));
    [Fact] public void PaymentController_HasIndexAction() => Assert.NotNull(ControllerType.GetMethod("Index"));
    [Fact] public void PaymentController_HasRequiredAuthorization() => Assert.Contains(ControllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true), a => ((AuthorizeAttribute)a).Roles == "Admin,Seller");
}
