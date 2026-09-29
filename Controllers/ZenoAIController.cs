using InventoryManagementSystem.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.IO.Compression;
using System.Xml.Linq;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class ZenoAIController : Controller
    {
        private readonly InventoryDbContext _context;
        private readonly Services.ZenoAIService.ZenoAIService _ai;

        public ZenoAIController(InventoryDbContext context, Services.ZenoAIService.ZenoAIService ai)
        {
            _context = context;
            _ai = ai;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(string message)
        {
            message = (message ?? "").Trim();
            var q = message.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(q))
                return Json(new { success = true, reply = "Please ask me something about inventory, stock movement, sales, billing or payments." });

            // Report generation is handled by the application so the download link is reliable.
            if (q.Contains("generate") && q.Contains("report"))
            {
                if (q.Contains("overall") || q.Contains("overall report") || q.Contains("complete") || q.Contains("full report"))
                {
                    return Json(new { success = true, reply = "Your overall Invenzo report is ready. <a href='/ZenoAI/OverallReport' target='_blank'>Download Overall Report (PDF)</a>." });
                }

                var type = q.Contains("payment") ? "payments" :
                           q.Contains("movement") ? "stock-movements" :
                           q.Contains("low stock") ? "low-stock" :
                           q.Contains("sale") ? "sales" :
                           q.Contains("purchase") ? "purchases" :
                           q.Contains("stock") || q.Contains("inventory") ? "inventory" : "sales";

                var format = (type == "purchases" || type == "sales") ? "Excel" : "CSV";
                return Json(new
                {
                    success = true,
                    reply = $"I can generate that report. <a href='/ZenoAI/GenerateReport?type={type}' target='_blank'>Download {type.Replace("-", " ")} report ({format})</a>."
                });
            }

            // =========================
            // LIVE DATABASE COUNTS
            // =========================
            var productCount = await _context.Products.CountAsync();
            var totalUnits = await _context.Products.SumAsync(p => (int?)p.Quantity) ?? 0;
            var lowStockCount = await _context.Products.CountAsync(p => p.Quantity <= p.ReorderLevel);
            var salesCount = await _context.Sales.CountAsync();
            var salesTotal = await _context.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            var categoryCount = await _context.Categories.CountAsync();
            var categoryNames = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .Select(c => c.CategoryName)
                .ToListAsync();
            var productDirectory = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.ProductName)
                .Select(p => new { p.ProductName, CategoryName = p.Category != null ? p.Category.CategoryName : "Uncategorized", p.Quantity })
                .ToListAsync();
            var invoiceCount = await _context.Billings.CountAsync();
            var supplierCount = await _context.Suppliers.CountAsync();
            var supplierNames = await _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .Select(s => s.SupplierName)
                .ToListAsync();

            var paidInvoiceCount = await _context.Billings.CountAsync(b => b.Status == "Paid");
            var outstandingInvoiceCount = await _context.Billings.CountAsync(b => b.Status != "Paid");
            var paidInvoices = await _context.Billings
                .Where(b => b.Status == "Paid")
                .SumAsync(b => (decimal?)b.GrandTotal) ?? 0;
            var outstandingInvoices = await _context.Billings
                .Where(b => b.Status != "Paid")
                .SumAsync(b => (decimal?)b.GrandTotal) ?? 0;

            var totalPaymentCount = await _context.Payments.CountAsync();
            var paidPaymentCount = await _context.Payments.CountAsync(p => p.Status == "Paid");
            var unpaidPaymentCount = await _context.Payments.CountAsync(p => p.Status != "Paid");
            var paidPayments = await _context.Payments
                .Where(p => p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var unpaidPayments = await _context.Payments
                .Where(p => p.Status != "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Payment method categories among paid payments.
            var paymentMethods = await _context.Payments
                .Where(p => p.Status == "Paid")
                .GroupBy(p => p.Method)
                .Select(g => new { Method = g.Key, Count = g.Count(), Amount = g.Sum(p => p.Amount) })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Stock movement totals and categories.
            var totalMovementCount = await _context.StockMovements.CountAsync();
            var stockInCount = await _context.StockMovements.CountAsync(sm => sm.MovementType == "IN");
            var stockOutCount = await _context.StockMovements.CountAsync(sm => sm.MovementType == "OUT");
            var stockInUnits = await _context.StockMovements
                .Where(sm => sm.MovementType == "IN")
                .SumAsync(sm => (int?)sm.Quantity) ?? 0;
            var stockOutUnits = await _context.StockMovements
                .Where(sm => sm.MovementType == "OUT")
                .SumAsync(sm => (int?)sm.Quantity) ?? 0;

            var recentMovements = await _context.StockMovements
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.MovementDate)
                .ThenByDescending(sm => sm.StockMovementId)
                .Take(10)
                .Select(sm => new
                {
                    Product = sm.Product != null ? sm.Product.ProductName : "Unknown product",
                    sm.MovementType,
                    sm.Quantity,
                    sm.MovementDate,
                    sm.Reference
                })
                .ToListAsync();

            var paymentMethodContext = paymentMethods.Count == 0
                ? "None"
                : string.Join("; ", paymentMethods.Select(x => $"{x.Method}: {x.Count} paid payment(s), ₹{x.Amount:N2}"));

            var categoryContext = categoryNames.Count == 0
                ? "None"
                : string.Join(", ", categoryNames);

            var recentMovementContext = recentMovements.Count == 0
                ? "None"
                : string.Join("; ", recentMovements.Select(x =>
                    $"{x.MovementDate:yyyy-MM-dd}: {x.MovementType} {x.Quantity} unit(s) of {x.Product} ({x.Reference})"));

            var businessContext =
                $"Product types: {productCount}; Total units currently in stock: {totalUnits:N0}; Low-stock products: {lowStockCount}; " +
                $"Sales count: {salesCount}; Sales value: ₹{salesTotal:N2}; Categories: {categoryCount}; Category names: {categoryContext}; " +
                $"Suppliers: {supplierCount}; Supplier names: {(supplierNames.Count == 0 ? "None" : string.Join(", ", supplierNames))}; Invoices: {invoiceCount}; " +
                $"Paid invoices: {paidInvoiceCount} worth ₹{paidInvoices:N2}; Outstanding invoices: {outstandingInvoiceCount} worth ₹{outstandingInvoices:N2}; " +
                $"Payments: {totalPaymentCount} total; {paidPaymentCount} paid; {unpaidPaymentCount} unpaid/pending; " +
                $"Paid payment amount: ₹{paidPayments:N2}; Unpaid/pending payment amount: ₹{unpaidPayments:N2}; " +
                $"Paid payment methods: {paymentMethodContext}; " +
                $"Stock movements: {totalMovementCount} total; {stockInCount} IN movement(s) / {stockInUnits:N0} units; " +
                $"{stockOutCount} OUT movement(s) / {stockOutUnits:N0} units; Recent movements: {recentMovementContext}.";

            // =========================
            // RELIABLE DETERMINISTIC ANSWERS
            // These run before the optional AI API so the requested database figures
            // always come directly from Invenzo's live database.
            // =========================
            // Category lookup: a category name can be typed directly or mentioned in a question.
            // The lookup is exact against the live database and returns every associated product.
            var matchedCategory = categoryNames
                .OrderByDescending(name => name.Length)
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name) &&
                    (q.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) ||
                     q.Contains($" {name.Trim().ToLowerInvariant()} ") ||
                     q.StartsWith(name.Trim().ToLowerInvariant() + " ") ||
                     q.EndsWith(" " + name.Trim().ToLowerInvariant())));

            if (matchedCategory != null &&
                !q.Contains("how many categor") && !q.Contains("number of categor") &&
                !q.Contains("categories"))
            {
                var categoryProducts = await _context.Products
                    .Where(p => p.Category != null && p.Category.CategoryName == matchedCategory)
                    .OrderBy(p => p.ProductName)
                    .Select(p => new { p.ProductName, p.Quantity })
                    .ToListAsync();

                var productText = categoryProducts.Count == 0
                    ? "No products are currently assigned to this category."
                    : string.Join(", ", categoryProducts.Select(p => $"{p.ProductName} ({p.Quantity} in stock)"));

                return Json(new
                {
                    success = true,
                    reply = $"Category '{matchedCategory}' has {categoryProducts.Count} product(s): {productText}."
                });
            }

            // Product lookup: when a product name is typed, tell the user its category.
            var matchedProduct = productDirectory
                .OrderByDescending(p => p.ProductName.Length)
                .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ProductName) &&
                    (q.Equals(p.ProductName.Trim(), StringComparison.OrdinalIgnoreCase) ||
                     q.Contains(p.ProductName.Trim().ToLowerInvariant())));

            if (matchedProduct != null)
            {
                return Json(new
                {
                    success = true,
                    reply = $"Product '{matchedProduct.ProductName}' belongs to the '{matchedProduct.CategoryName}' category. Current stock: {matchedProduct.Quantity}."
                });
            }

            if ((q.Contains("category") || q.Contains("categories")) &&
                (q.Contains("how many") || q.Contains("number") || q.Contains("list") ||
                 q.Contains("name") || q.Contains("names") || q.Contains("categories")))
            {
                var names = categoryNames.Count == 0 ? "No categories found." : string.Join(", ", categoryNames);
                return Json(new
                {
                    success = true,
                    reply = $"Invenzo has {categoryCount} categor{(categoryCount == 1 ? "y" : "ies")}: {names}."
                });
            }

            if ((q.Contains("how many") || q.Contains("number of") || q.Contains("count")) &&
                (q.Contains("sales") || q.Contains("sale")))
            {
                return Json(new
                {
                    success = true,
                    reply = $"There are {salesCount} sale(s) recorded in Invenzo, with total sales value of ₹{salesTotal:N2}."
                });
            }

            if ((q.Contains("how many") || q.Contains("number of") || q.Contains("count")) &&
                (q.Contains("products") || q.Contains("product")))
            {
                return Json(new
                {
                    success = true,
                    reply = $"There are {productCount} product(s) in Invenzo, with {totalUnits:N0} total units currently in stock."
                });
            }

            if (q.Contains("supplier") || q.Contains("suppliers"))
            {
                var names = supplierNames.Count == 0 ? "No suppliers found." : string.Join(", ", supplierNames);
                return Json(new
                {
                    success = true,
                    reply = $"Invenzo has {supplierCount} supplier(s). Supplier names: {names}."
                });
            }

            if (q.Contains("stock movement") || q.Contains("stock movements") ||
                q.Contains("movement") || q.Contains("stock in") || q.Contains("stock out"))
            {
                var latest = recentMovements.Count == 0
                    ? "No stock movements have been recorded yet."
                    : " Recent: " + string.Join("; ", recentMovements.Take(5).Select(x =>
                        $"{x.MovementType} {x.Quantity} {x.Product}"));

                return Json(new
                {
                    success = true,
                    reply = $"Stock movement summary: {totalMovementCount} total movements — {stockInCount} IN ({stockInUnits:N0} units) and {stockOutCount} OUT ({stockOutUnits:N0} units).{latest}"
                });
            }

            if (q.Contains("payment") || q.Contains("paid") || q.Contains("unpaid") ||
                q.Contains("pending payment") || q.Contains("payment count") || q.Contains("how many payments"))
            {
                var categories = paymentMethods.Count == 0
                    ? "No paid payment methods recorded yet."
                    : string.Join(" | ", paymentMethods.Select(x => $"{x.Method}: {x.Count} paid (₹{x.Amount:N2})"));

                return Json(new
                {
                    success = true,
                    reply = $"Payment summary: {totalPaymentCount} total payment record(s). Paid: {paidPaymentCount} (₹{paidPayments:N2}). Unpaid/Pending: {unpaidPaymentCount} (₹{unpaidPayments:N2}). Paid payment methods: {categories}."
                });
            }

            // Optional real AI layer. It receives the same live database context,
            // so it can explain the figures without inventing values.
            var aiReply = await _ai.AskAsync(message, businessContext);
            if (!string.IsNullOrWhiteSpace(aiReply))
                return Json(new { success = true, reply = aiReply, ai = true });

            if (q.Contains("low stock") || q.Contains("reorder"))
            {
                var items = await _context.Products
                    .Where(p => p.Quantity <= p.ReorderLevel)
                    .OrderBy(p => p.Quantity)
                    .Take(10)
                    .ToListAsync();

                var reply = items.Count == 0
                    ? "No products are currently below their reorder level."
                    : "Low-stock products: " + string.Join(", ", items.Select(p => $"{p.ProductName} ({p.Quantity})")) + ".";

                return Json(new { success = true, reply });
            }

            if (q.Contains("stock") || q.Contains("inventory"))
                return Json(new { success = true, reply = $"Inventory has {productCount} product types and {totalUnits:N0} total units." });

            if (q.Contains("sale") || q.Contains("revenue"))
                return Json(new { success = true, reply = $"There are {salesCount} sales with total sales value of ₹{salesTotal:N2}." });

            if (q.Contains("billing") || q.Contains("invoice"))
                return Json(new { success = true, reply = $"Billing: {invoiceCount} invoice(s), {paidInvoiceCount} paid and {outstandingInvoiceCount} outstanding. Paid value ₹{paidInvoices:N2}; outstanding value ₹{outstandingInvoices:N2}." });

            return Json(new
            {
                success = true,
                reply = "I'm ZenoAI. Ask me about stock, stock movement, low stock, sales, billing, payments, paid/unpaid payment counts, payment methods, or say “Generate sales report”."
            });
        }

        [HttpGet]
        public async Task<IActionResult> GenerateReport(string type = "sales")
        {
            type = type.ToLowerInvariant();

            // Sales and purchases are downloadable as real Excel workbooks.
            if (type == "purchases")
            {
                var purchases = await _context.Purchases
                    .Include(p => p.Supplier)
                    .Include(p => p.PurchaseItems)
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync();

                var rows = purchases.Select(p => new[]
                {
                    p.PurchaseId.ToString(),
                    p.PurchaseDate.ToString("yyyy-MM-dd"),
                    p.Supplier?.SupplierName ?? "",
                    p.PurchaseItems?.Count.ToString() ?? "0",
                    p.TotalAmount.ToString("0.00")
                }).ToList();

                var bytes = BuildExcelWorkbook(
                    "Purchases",
                    new[] { "Purchase ID", "Purchase Date", "Supplier", "Item Lines", "Total Amount" },
                    rows);

                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Invenzo-Purchase-Report-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
            }

            if (type == "sales")
            {
                var sales = await _context.Sales
                    .Include(s => s.SaleItems)
                    .ThenInclude(i => i.Product)
                    .OrderByDescending(s => s.SaleDate)
                    .ToListAsync();

                var rows = sales.Select(s => new[]
                {
                    s.SaleId.ToString(),
                    s.SaleDate.ToString("yyyy-MM-dd"),
                    s.SaleItems?.Count.ToString() ?? "0",
                    s.TotalAmount.ToString("0.00")
                }).ToList();

                var bytes = BuildExcelWorkbook(
                    "Sales",
                    new[] { "Sale ID", "Sale Date", "Item Lines", "Total Amount" },
                    rows);

                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Invenzo-Sales-Report-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
            }

            var csv = new StringBuilder();

            if (type == "inventory")
            {
                csv.AppendLine("Product,Category,Supplier,Quantity,ReorderLevel,PurchasePrice,SellingPrice");
                var products = await _context.Products.Include(p => p.Category).Include(p => p.Supplier).OrderBy(p => p.ProductName).ToListAsync();
                foreach (var p in products)
                    csv.AppendLine($"{Csv(p.ProductName)},{Csv(p.Category?.CategoryName ?? "Uncategorized")},{Csv(p.Supplier?.SupplierName ?? "")},{p.Quantity},{p.ReorderLevel},{p.PurchasePrice:0.00},{p.SellingPrice:0.00}");
            }
            else if (type == "low-stock")
            {
                csv.AppendLine("Product,Category,CurrentStock,ReorderLevel,Shortage");
                var products = await _context.Products.Include(p => p.Category).Where(p => p.Quantity <= p.ReorderLevel).OrderBy(p => p.Quantity).ToListAsync();
                foreach (var p in products)
                    csv.AppendLine($"{Csv(p.ProductName)},{Csv(p.Category?.CategoryName ?? "Uncategorized")},{p.Quantity},{p.ReorderLevel},{Math.Max(p.ReorderLevel - p.Quantity, 0)}");
            }
            else if (type == "payments")
            {
                csv.AppendLine("PaymentId,InvoiceNumber,Amount,Method,Status,TransactionReference,PaidAt");
                var payments = await _context.Payments.Include(p => p.Billing).OrderByDescending(p => p.PaidAt).ToListAsync();
                foreach (var p in payments)
                    csv.AppendLine($"{p.PaymentId},{Csv(p.Billing?.InvoiceNumber ?? "")},{p.Amount:0.00},{Csv(p.Method)},{Csv(p.Status)},{Csv(p.TransactionReference)},{p.PaidAt:yyyy-MM-dd HH:mm:ss}");
            }
            else if (type == "stock-movements")
            {
                csv.AppendLine("StockMovementId,Product,MovementType,Quantity,MovementDate,Reference");
                var movements = await _context.StockMovements.Include(sm => sm.Product).OrderByDescending(sm => sm.MovementDate).ToListAsync();
                foreach (var sm in movements)
                    csv.AppendLine($"{sm.StockMovementId},{Csv(sm.Product?.ProductName ?? "")},{Csv(sm.MovementType)},{sm.Quantity},{sm.MovementDate:yyyy-MM-dd HH:mm:ss},{Csv(sm.Reference ?? "")}");
            }
            else
            {
                csv.AppendLine("Product,Quantity,ReorderLevel,SellingPrice");
                var products = await _context.Products.OrderBy(p => p.ProductName).ToListAsync();
                foreach (var p in products)
                    csv.AppendLine($"{Csv(p.ProductName)},{p.Quantity},{p.ReorderLevel},{p.SellingPrice:0.00}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv",
                $"Invenzo-{type}-report-{DateTime.Now:yyyyMMdd-HHmm}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> OverallReport()
        {
            var now = DateTime.Now;
            var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            var months = Enumerable.Range(0, 6).Select(i => startMonth.AddMonths(i)).ToList();

            var products = await _context.Products.Include(p => p.Category).Include(p => p.Supplier).OrderBy(p => p.ProductName).ToListAsync();
            var sales = await _context.Sales.Include(s => s.SaleItems).ThenInclude(i => i.Product).OrderByDescending(s => s.SaleDate).ToListAsync();
            var purchases = await _context.Purchases.Include(p => p.Supplier).Include(p => p.PurchaseItems).OrderByDescending(p => p.PurchaseDate).ToListAsync();
            var movements = await _context.StockMovements.Include(m => m.Product).OrderByDescending(m => m.MovementDate).ToListAsync();
            var payments = await _context.Payments.Include(p => p.Billing).OrderByDescending(p => p.PaidAt).ToListAsync();

            var lowStock = products.Where(p => p.Quantity <= p.ReorderLevel).OrderBy(p => p.Quantity).ToList();
            var inStock = products.Count(p => p.Quantity > p.ReorderLevel);
            var low = products.Count(p => p.Quantity > 0 && p.Quantity <= p.ReorderLevel);
            var outStock = products.Count(p => p.Quantity <= 0);
            var paid = payments.Where(p => p.Status == "Paid").ToList();
            var pending = payments.Where(p => p.Status != "Paid").ToList();
            var stockIn = movements.Where(m => string.Equals(m.MovementType, "IN", StringComparison.OrdinalIgnoreCase)).Sum(m => m.Quantity);
            var stockOut = movements.Where(m => string.Equals(m.MovementType, "OUT", StringComparison.OrdinalIgnoreCase)).Sum(m => m.Quantity);

            var monthlySales = months.Select(m => sales
                .Where(s => s.SaleDate.Year == m.Year && s.SaleDate.Month == m.Month)
                .Sum(s => s.TotalAmount)).ToList();

            var purchaseBySupplier = purchases
                .GroupBy(p => p.Supplier?.SupplierName ?? "Unknown")
                .Select(g => (Name: g.Key, Value: g.Sum(x => x.TotalAmount)))
                .OrderByDescending(x => x.Value).Take(8).ToList();

            var salesByMonth = months.Select((m, i) => (Name: m.ToString("MMM"), Value: monthlySales[i])).ToList();

            var stockParts = new List<(string Name, decimal Value)>
            {
                ("In Stock", inStock), ("Low Stock", low), ("Out of Stock", outStock)
            };

            var lowByCategory = lowStock
                .GroupBy(p => p.Category?.CategoryName ?? "Uncategorized")
                .Select(g => (Name: g.Key, Value: (decimal)g.Count()))
                .OrderByDescending(x => x.Value).Take(8).ToList();

            var movementParts = new List<(string Name, decimal Value)>
            {
                ("IN", stockIn), ("OUT", stockOut)
            };

            var paymentParts = new List<(string Name, decimal Value)>
            {
                ("Paid", paid.Count), ("Pending / Unpaid", pending.Count)
            };

            var pages = new List<string>();

            pages.Add(BuildPdfLineChartPage(
                "Invenzo Overall Business Report",
                $"Monthly Sales Graph | Generated {now:dd MMMM yyyy, hh:mm tt}",
                salesByMonth,
                $"Total sales value: Rs. {sales.Sum(s => s.TotalAmount):N2} | {sales.Count} sale records"));

            pages.Add(BuildPdfPieReportPage(
                "Purchase Report",
                "Purchase value by supplier",
                purchaseBySupplier,
                purchases.Take(18).Select(p => new[]
                {
                    p.PurchaseId.ToString(), p.PurchaseDate.ToString("dd MMM yyyy"),
                    p.Supplier?.SupplierName ?? "Unknown", $"Rs. {p.TotalAmount:N2}"
                }).ToList(),
                new[] { "ID", "Date", "Supplier", "Amount" },
                $"Total purchases: Rs. {purchases.Sum(p => p.TotalAmount):N2} | {purchases.Count} purchase records"));

            pages.Add(BuildPdfPieReportPage(
                "Sales Report",
                "Sales value by month",
                salesByMonth,
                sales.Take(18).Select(s => new[]
                {
                    s.SaleId.ToString(), s.SaleDate.ToString("dd MMM yyyy"), $"Rs. {s.TotalAmount:N2}"
                }).ToList(),
                new[] { "ID", "Date", "Amount" },
                $"Total sales: Rs. {sales.Sum(s => s.TotalAmount):N2} | {sales.Count} sale records"));

            pages.Add(BuildPdfPieReportPage(
                "Stock Report",
                "Current product stock health",
                stockParts,
                products.Take(18).Select(p => new[]
                {
                    p.ProductName, p.Category?.CategoryName ?? "Uncategorized", p.Quantity.ToString(), p.ReorderLevel.ToString()
                }).ToList(),
                new[] { "Product", "Category", "Current", "Reorder" },
                $"Products: {products.Count} | In Stock: {inStock} | Low Stock: {low} | Out of Stock: {outStock}"));

            pages.Add(BuildPdfPieReportPage(
                "Low Stock Report",
                "Low-stock products by category",
                lowByCategory.Count == 0 ? new List<(string Name, decimal Value)> { ("No low stock", 1) } : lowByCategory,
                lowStock.Take(24).Select(p => new[]
                {
                    p.ProductName, p.Category?.CategoryName ?? "Uncategorized", p.Quantity.ToString(), p.ReorderLevel.ToString()
                }).ToList(),
                new[] { "Product", "Category", "Current", "Reorder" },
                $"Products needing attention: {lowStock.Count}"));

            pages.Add(BuildPdfPieReportPage(
                "Stock Movement Report",
                "IN vs OUT movement units",
                movementParts,
                movements.Take(24).Select(m => new[]
                {
                    m.MovementDate.ToString("dd MMM yyyy"), m.Product?.ProductName ?? "Unknown", m.MovementType, m.Quantity.ToString()
                }).ToList(),
                new[] { "Date", "Product", "Type", "Quantity" },
                $"Total records: {movements.Count} | IN: {stockIn:N0} units | OUT: {stockOut:N0} units"));

            pages.Add(BuildPdfPieReportPage(
                "Payments Report",
                "Paid vs pending / unpaid payments",
                paymentParts,
                payments.Take(24).Select(p => new[]
                {
                    p.PaymentId.ToString(), p.Billing?.InvoiceNumber ?? "-", p.Status,
                    $"Rs. {p.Amount:N2}"
                }).ToList(),
                new[] { "ID", "Invoice", "Status", "Amount" },
                $"Paid: {paid.Count} | Pending / Unpaid: {pending.Count} | Paid amount: Rs. {paid.Sum(p => p.Amount):N2} | Pending amount: Rs. {pending.Sum(p => p.Amount):N2}"));

            var pdf = BuildPdfDocument(pages);
            return File(pdf, "application/pdf", $"Invenzo-Overall-Report-{now:yyyyMMdd-HHmm}.pdf");
        }

        private static string BuildPdfLineChartPage(string title, string subtitle, List<(string Name, decimal Value)> data, string footer)
        {
            var c = new StringBuilder();
            PdfText(c, 42, 800, title, 22, true);
            PdfText(c, 42, 778, subtitle, 10, false, 0.39, 0.45, 0.55);
            PdfText(c, 42, 744, "Monthly sales value", 12, true);
            DrawLineChartPdf(c, 52, 455, 500, 250, data);
            PdfText(c, 42, 420, footer, 10, false, 0.39, 0.45, 0.55);
            PdfText(c, 42, 385, "The chart uses the latest six calendar months from the Invenzo database.", 9, false, 0.39, 0.45, 0.55);
            return c.ToString();
        }

        private static string BuildPdfPieReportPage(string title, string subtitle, List<(string Name, decimal Value)> data, List<string[]> rows, string[] headers, string footer)
        {
            var c = new StringBuilder();
            PdfText(c, 42, 800, title, 22, true);
            PdfText(c, 42, 778, subtitle, 10, false, 0.39, 0.45, 0.55);
            DrawPieChartPdf(c, 145, 655, 105, data);
            DrawPdfLegend(c, 275, 735, data);
            PdfText(c, 42, 525, footer, 10, false, 0.39, 0.45, 0.55);
            DrawPdfTable(c, 42, 500, 511, headers, rows, 16);
            return c.ToString();
        }

        private static void PdfText(StringBuilder c, double x, double y, string text, double size, bool bold, double r = 0.09, double g = 0.15, double b = 0.33)
        {
            var font = bold ? "/F2" : "/F1";
            var safe = (text ?? string.Empty).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            c.Append($"BT {font} {size:0.##} Tf {r:0.###} {g:0.###} {b:0.###} rg {x:0.##} {y:0.##} Td ({safe}) Tj ET\n");
        }

        private static void DrawLineChartPdf(StringBuilder c, double x, double y, double w, double h, List<(string Name, decimal Value)> data)
        {
            var max = Math.Max(data.Count == 0 ? 0 : data.Max(d => (double)d.Value), 1);
            c.Append("0.90 0.93 0.98 RG 0.6 w\n");
            for (var i = 0; i <= 4; i++)
            {
                var gy = y + h * i / 4.0;
                c.Append($"{x:0.##} {gy:0.##} m {x + w:0.##} {gy:0.##} l S\n");
            }
            if (data.Count == 0) return;
            var points = new List<(double X, double Y)>();
            for (var i = 0; i < data.Count; i++)
            {
                var px = data.Count == 1 ? x + w / 2 : x + w * i / (data.Count - 1.0);
                var py = y + h * ((double)data[i].Value / max);
                points.Add((px, py));
            }
            var area = new StringBuilder();
            area.Append($"{points[0].X:0.##} {y:0.##} m ");
            foreach (var p in points) area.Append($"{p.X:0.##} {p.Y:0.##} l ");
            area.Append($"{points[^1].X:0.##} {y:0.##} l h");
            c.Append($"0.84 0.91 1.0 rg {area} f\n");
            c.Append("0.13 0.39 0.92 RG 2.4 w\n");
            c.Append($"{points[0].X:0.##} {points[0].Y:0.##} m ");
            foreach (var p in points.Skip(1)) c.Append($"{p.X:0.##} {p.Y:0.##} l ");
            c.Append("S\n");
            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i];
                PdfCircle(c, p.X, p.Y, 4, 0.13, 0.39, 0.92, true);
                PdfText(c, p.X - 12, y - 22, data[i].Name, 9, false, 0.39, 0.45, 0.55);
                PdfText(c, p.X - 22, p.Y + 10, $"Rs.{data[i].Value:N0}", 8, false, 0.13, 0.25, 0.55);
            }
        }

        private static void DrawPieChartPdf(StringBuilder c, double cx, double cy, double radius, List<(string Name, decimal Value)> data)
        {
            var total = (double)data.Sum(d => d.Value);
            if (total <= 0)
            {
                PdfCircle(c, cx, cy, radius, 0.90, 0.93, 0.96, true);
                return;
            }
            var colors = new (double R, double G, double B)[]
            {
                (0.13,0.39,0.92),(0.10,0.68,0.45),(0.98,0.63,0.12),(0.94,0.27,0.27),(0.55,0.32,0.85),(0.02,0.65,0.78)
            };
            var angle = -Math.PI / 2;
            var ci = 0;
            foreach (var item in data.Where(d => d.Value > 0))
            {
                var delta = (double)item.Value / total * Math.PI * 2;
                var steps = Math.Max(8, (int)Math.Ceiling(delta * 12));
                var color = colors[ci++ % colors.Length];
                c.Append($"{color.R:0.###} {color.G:0.###} {color.B:0.###} rg {cx:0.##} {cy:0.##} m ");
                for (var i = 0; i <= steps; i++)
                {
                    var a = angle + delta * i / steps;
                    var px = cx + radius * Math.Cos(a);
                    var py = cy + radius * Math.Sin(a);
                    c.Append($"{px:0.##} {py:0.##} l ");
                }
                c.Append("h f\n");
                angle += delta;
            }
            PdfCircle(c, cx, cy, radius * 0.48, 1, 1, 1, true);
            PdfText(c, cx - 24, cy + 2, $"{data.Sum(d => d.Value):N0}", 15, true);
            PdfText(c, cx - 23, cy - 15, "Total", 8, false, 0.39, 0.45, 0.55);
        }

        private static void PdfCircle(StringBuilder c, double cx, double cy, double radius, double r, double g, double b, bool fill)
        {
            const double k = 0.5522847498;
            var x = radius * k;
            c.Append($"{r:0.###} {g:0.###} {b:0.###} rg {cx + radius:0.##} {cy:0.##} m ");
            c.Append($"{cx + radius:0.##} {cy + x:0.##} {cx + x:0.##} {cy + radius:0.##} {cx:0.##} {cy + radius:0.##} c ");
            c.Append($"{cx - x:0.##} {cy + radius:0.##} {cx - radius:0.##} {cy + x:0.##} {cx - radius:0.##} {cy:0.##} c ");
            c.Append($"{cx - radius:0.##} {cy - x:0.##} {cx - x:0.##} {cy - radius:0.##} {cx:0.##} {cy - radius:0.##} c ");
            c.Append($"{cx + x:0.##} {cy - radius:0.##} {cx + radius:0.##} {cy - x:0.##} {cx + radius:0.##} {cy:0.##} c {(fill ? "f" : "S")}\n");
        }

        private static void DrawPdfLegend(StringBuilder c, double x, double y, List<(string Name, decimal Value)> data)
        {
            var colors = new (double R, double G, double B)[]
            {
                (0.13,0.39,0.92),(0.10,0.68,0.45),(0.98,0.63,0.12),(0.94,0.27,0.27),(0.55,0.32,0.85),(0.02,0.65,0.78)
            };
            var i = 0;
            foreach (var item in data)
            {
                var color = colors[i++ % colors.Length];
                c.Append($"{color.R:0.###} {color.G:0.###} {color.B:0.###} rg {x:0.##} {y - i * 22:0.##} 9 9 re f\n");
                PdfText(c, x + 16, y - i * 22 + 1, $"{item.Name}: {item.Value:N0}", 9, false);
            }
        }

        private static void DrawPdfTable(StringBuilder c, double x, double yTop, double width, string[] headers, List<string[]> rows, int rowHeight)
        {
            var cols = headers.Length;
            if (cols == 0) return;
            var colWidth = width / cols;
            c.Append($"0.94 0.96 0.99 rg {x:0.##} {yTop - rowHeight:0.##} {width:0.##} {rowHeight:0.##} re f\n");
            for (var i = 0; i < cols; i++) PdfText(c, x + i * colWidth + 4, yTop - 11, headers[i], 8, true);
            var y = yTop - rowHeight;
            foreach (var row in rows.Take(18))
            {
                y -= rowHeight;
                if (y < 35) break;
                c.Append($"0.90 0.91 0.94 RG 0.5 w {x:0.##} {y:0.##} m {x + width:0.##} {y:0.##} l S\n");
                for (var i = 0; i < cols; i++)
                {
                    var value = i < row.Length ? row[i] ?? "" : "";
                    if (value.Length > 30) value = value[..27] + "...";
                    PdfText(c, x + i * colWidth + 4, y + 5, value, 7.5, false, 0.13, 0.17, 0.28);
                }
            }
        }

        private static byte[] BuildPdfDocument(List<string> pageContents)
        {
            var objects = new List<string>();
            objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
            objects.Add("<< /Type /Pages /Kids [" + string.Join(" ", Enumerable.Range(0, pageContents.Count).Select(i => $"{5 + i * 2} 0 R")) + $"] /Count {pageContents.Count} >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

            foreach (var content in pageContents)
            {
                var pageObjectNumber = objects.Count + 1;
                var contentObjectNumber = pageObjectNumber + 1;
                var length = Encoding.ASCII.GetByteCount(content);
                objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {contentObjectNumber} 0 R >>");
                objects.Add($"<< /Length {length} >>\nstream\n{content}endstream");
            }

            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, new UTF8Encoding(false), leaveOpen: true);
            writer.Write("%PDF-1.4\n");
            writer.Flush();
            var offsets = new List<long> { 0 };
            for (var i = 0; i < objects.Count; i++)
            {
                writer.Flush();
                offsets.Add(ms.Position);
                writer.Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }
            writer.Flush();
            var xref = ms.Position;
            writer.Write($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
            for (var i = 1; i <= objects.Count; i++) writer.Write($"{offsets[i]:D10} 00000 n \n");
            writer.Write($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            writer.Flush();
            return ms.ToArray();
        }

        private static string SvgLineChart(List<(string Name, decimal Value)> data)
        {
            const double W=760,H=250,L=50,R=20,T=25,B=45; var max=Math.Max((double)(data.Count==0?0:data.Max(x=>x.Value)),1); var uw=W-L-R; var uh=H-T-B;
            var sb=new StringBuilder($"<svg class='chart' viewBox='0 0 {W} {H}' role='img'>");
            for(int g=0;g<=4;g++){var y=T+uh*g/4; sb.Append($"<line x1='{L}' y1='{y}' x2='{W-R}' y2='{y}' stroke='#e5e7eb'/>");}
            var pts=new List<string>(); for(int i=0;i<data.Count;i++){var x=data.Count<=1?L:L+uw*i/(data.Count-1); var y=T+uh-(double)data[i].Value/max*uh; pts.Add($"{x:0.##},{y:0.##}");}
            if(pts.Count>0){var area=$"{L},{T+uh} {string.Join(" ",pts)} {W-R},{T+uh}"; sb.Append($"<polygon points='{area}' fill='#dbeafe' opacity='.9'/><polyline points='{string.Join(" ",pts)}' fill='none' stroke='#2563eb' stroke-width='4'/>"); for(int i=0;i<pts.Count;i++){var a=pts[i].Split(','); sb.Append($"<circle cx='{a[0]}' cy='{a[1]}' r='5' fill='#2563eb'/><text x='{a[0]}' y='{H-12}' text-anchor='middle' font-size='13' fill='#64748b'>{System.Net.WebUtility.HtmlEncode(data[i].Name)}</text>");}}
            sb.Append("</svg>"); return sb.ToString();
        }

        private static string SvgBarChart(List<(string Name, decimal Value)> data, string label)
        {
            const double W=760,H=260,L=55,R=20,T=20,B=55; var max=Math.Max((double)(data.Count==0?0:data.Max(x=>x.Value)),1); var uw=W-L-R; var uh=H-T-B; var sb=new StringBuilder($"<svg class='chart' viewBox='0 0 {W} {H}' role='img'>");
            for(int g=0;g<=4;g++){var y=T+uh*g/4; sb.Append($"<line x1='{L}' y1='{y}' x2='{W-R}' y2='{y}' stroke='#e5e7eb'/>");}
            var bw=data.Count==0?20:Math.Min(70,uw/Math.Max(data.Count,1)*0.62); for(int i=0;i<data.Count;i++){var x=L+(i+.5)*uw/data.Count-bw/2; var bh=(double)data[i].Value/max*uh; var y=T+uh-bh; sb.Append($"<rect x='{x:0.##}' y='{y:0.##}' width='{bw:0.##}' height='{bh:0.##}' rx='5' fill='#2563eb'/><text x='{x+bw/2:0.##}' y='{H-16}' text-anchor='middle' font-size='12' fill='#64748b'>{System.Net.WebUtility.HtmlEncode(data[i].Name)}</text>");}
            sb.Append($"<text x='{L}' y='14' font-size='12' fill='#64748b'>{System.Net.WebUtility.HtmlEncode(label)}</text></svg>"); return sb.ToString();
        }

        private static string SvgPieChart(List<(string Name, decimal Value)> data, string label)
        {
            const double cx=145,cy=125,r=85; var total=(double)data.Sum(x=>x.Value); var sb=new StringBuilder("<div style='display:flex;align-items:center;gap:25px;flex-wrap:wrap'><svg width='300' height='250' viewBox='0 0 300 250' role='img'>");
            if(total<=0){sb.Append($"<circle cx='{cx}' cy='{cy}' r='{r}' fill='#e5e7eb'/>");}
            else{double angle=-Math.PI/2; string[] fills={"#2563eb","#22c55e","#f59e0b","#ef4444","#8b5cf6","#06b6d4"}; int idx=0; foreach(var item in data.Where(x=>x.Value>0)){var delta=(double)item.Value/total*Math.PI*2; var end=angle+delta; var large=delta>Math.PI?1:0; var x1=cx+r*Math.Cos(angle); var y1=cy+r*Math.Sin(angle); var x2=cx+r*Math.Cos(end); var y2=cy+r*Math.Sin(end); var color=fills[idx%fills.Length]; sb.Append($"<path d='M {cx} {cy} L {x1:0.##} {y1:0.##} A {r} {r} 0 {large} 1 {x2:0.##} {y2:0.##} Z' fill='{color}' stroke='#fff' stroke-width='2'/>"); angle=end; idx++;} }
            sb.Append($"<circle cx='{cx}' cy='{cy}' r='48' fill='#fff'/><text x='{cx}' y='{cy-3}' text-anchor='middle' font-size='13' fill='#64748b'>{System.Net.WebUtility.HtmlEncode(label)}</text><text x='{cx}' y='{cy+17}' text-anchor='middle' font-size='20' font-weight='700' fill='#172554'>{data.Sum(x=>x.Value):N0}</text></svg><div class='legend'>");
            string[] legendColors={"#2563eb","#22c55e","#f59e0b","#ef4444","#8b5cf6","#06b6d4"}; int li=0; foreach(var item in data){sb.Append($"<div><span class='dot' style='background:{legendColors[li%legendColors.Length]}'></span>{System.Net.WebUtility.HtmlEncode(item.Name)}: <b>{item.Value:N0}</b></div>"); li++;} sb.Append("</div></div>"); return sb.ToString();
        }

        private static byte[] BuildExcelWorkbook(string sheetName, string[] headers, List<string[]> rows)
        {
            XNamespace main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            XNamespace rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            XNamespace pkgRel = "http://schemas.openxmlformats.org/package/2006/relationships";
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                void Add(string path, string content)
                {
                    var e = zip.CreateEntry(path);
                    using var w = new StreamWriter(e.Open(), new UTF8Encoding(false));
                    w.Write(content);
                }
                Add("[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
                Add("_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
                Add("xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
                Add("xl/workbook.xml", $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"{main}\" xmlns:r=\"{rel}\"><sheets><sheet name=\"{System.Security.SecurityElement.Escape(sheetName)}\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                var sb = new StringBuilder($"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"{main}\"><sheetData>");
                void Row(string[] vals, int r)
                {
                    sb.Append($"<row r=\"{r}\">");
                    for(int i=0;i<vals.Length;i++) sb.Append($"<c r=\"{Column(i+1)}{r}\" t=\"inlineStr\"><is><t>{System.Security.SecurityElement.Escape(vals[i])}</t></is></c>");
                    sb.Append("</row>");
                }
                Row(headers,1); for(int i=0;i<rows.Count;i++) Row(rows[i],i+2); sb.Append("</sheetData></worksheet>");
                Add("xl/worksheets/sheet1.xml", sb.ToString());
            }
            return ms.ToArray();

            static string Column(int n){var s="";while(n>0){n--;s=(char)('A'+n%26)+s;n/=26;}return s;}
        }

        private static string Csv(string value) => $"\"{(value ?? "").Replace("\"", "\"\"")}\"";

    }
}
