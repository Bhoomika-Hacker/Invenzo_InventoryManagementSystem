using InventoryManagementSystem.DAL;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class BillingController : Controller
    {
        private readonly InventoryDbContext _context;
        public BillingController(InventoryDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var bills = await _context.Billings.Include(b => b.Sale).Include(b => b.Payments)
                .OrderByDescending(b => b.IssueDate).ToListAsync();
            return View(bills);
        }

        [HttpGet]
        public async Task<IActionResult> Generate(int saleId)
        {
            var sale = await _context.Sales.Include(s => s.SaleItems).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.SaleId == saleId);
            if (sale == null) return NotFound();

            var billing = await _context.Billings.Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.SaleId == saleId);

            if (billing == null)
            {
                var tax = Math.Round(sale.TotalAmount * .18m, 2);
                billing = new Billing {
                    SaleId = sale.SaleId, InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{sale.SaleId:D5}",
                    IssueDate = DateTime.Now, CustomerName = "Walk-in Customer",
                    Subtotal = sale.TotalAmount, TaxRate = 18m, TaxAmount = tax,
                    GrandTotal = sale.TotalAmount + tax, Status = "Outstanding"
                };
                _context.Billings.Add(billing);
                await _context.SaveChangesAsync();
                _context.Payments.Add(new Payment {
                    BillingId = billing.BillingId, Amount = billing.GrandTotal,
                    Method = "Pending", Status = "Pending",
                    TransactionReference = $"PENDING-{billing.InvoiceNumber}", PaidAt = billing.IssueDate
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = billing.BillingId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var billing = await _context.Billings.Include(b => b.Sale).ThenInclude(s => s!.SaleItems)
                .ThenInclude(i => i.Product).Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.BillingId == id);
            if (billing == null) return NotFound();
            return View(billing);
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
                return RedirectToAction(nameof(Details), new { id });
            }

            var payment = billing.Payments.FirstOrDefault(p => p.Status == "Pending");
            if (payment == null)
            {
                payment = new Payment { BillingId = id, Amount = billing.GrandTotal };
                _context.Payments.Add(payment);
            }
            payment.Method = string.IsNullOrWhiteSpace(method) ? "Cash" : method;
            payment.Status = "Paid";
            payment.PaidAt = DateTime.Now;
            payment.TransactionReference = $"PAY-{DateTime.Now:yyyyMMddHHmmssfff}";
            billing.Status = "Paid";
            await _context.SaveChangesAsync();

            TempData["BillingMessage"] = "Payment recorded successfully. Invoice is now Paid.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
