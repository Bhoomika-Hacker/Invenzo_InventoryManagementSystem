using InventoryManagementSystem.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class PaymentController : Controller
    {
        private readonly InventoryDbContext _context;
        public PaymentController(InventoryDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments.Include(p => p.Billing)
                .OrderByDescending(p => p.PaidAt).ToListAsync();
            return View(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id, string method)
        {
            var billing = await _context.Billings.Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.BillingId == id);
            if (billing == null) return NotFound();

            if (billing.Status == "Paid")
            {
                TempData["BillingMessage"] = "This invoice is already fully paid.";
                return RedirectToAction("Details", "Billing", new { id });
            }

            var payment = billing.Payments.FirstOrDefault(p => p.Status == "Pending");
            if (payment == null)
            {
                payment = new Models.Payment { BillingId = id, Amount = billing.GrandTotal };
                _context.Payments.Add(payment);
            }

            payment.Method = string.IsNullOrWhiteSpace(method) ? "Cash" : method;
            payment.Status = "Paid";
            payment.PaidAt = DateTime.Now;
            payment.TransactionReference = $"PAY-{DateTime.Now:yyyyMMddHHmmssfff}";
            billing.Status = "Paid";

            await _context.SaveChangesAsync();
            TempData["BillingMessage"] = "Payment recorded successfully. Invoice is now Paid.";
            return RedirectToAction("Details", "Billing", new { id });
        }
    }
}
