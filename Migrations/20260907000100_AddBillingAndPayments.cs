using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagementSystem.Migrations
{
    public partial class AddBillingAndPayments : Migration
    {
        protected override void Up(MigrationBuilder m)
        {
            m.CreateTable("Billings", t => new {
                BillingId = t.Column<int>(type:"int", nullable:false).Annotation("SqlServer:Identity","1, 1"),
                InvoiceNumber = t.Column<string>(type:"nvarchar(40)", maxLength:40, nullable:false),
                SaleId = t.Column<int>(type:"int", nullable:false),
                IssueDate = t.Column<DateTime>(type:"datetime2", nullable:false),
                CustomerName = t.Column<string>(type:"nvarchar(120)", maxLength:120, nullable:false),
                CustomerEmail = t.Column<string>(type:"nvarchar(160)", maxLength:160, nullable:true),
                Subtotal = t.Column<decimal>(type:"decimal(18,2)", nullable:false),
                TaxRate = t.Column<decimal>(type:"decimal(18,2)", nullable:false),
                TaxAmount = t.Column<decimal>(type:"decimal(18,2)", nullable:false),
                Discount = t.Column<decimal>(type:"decimal(18,2)", nullable:false),
                GrandTotal = t.Column<decimal>(type:"decimal(18,2)", nullable:false),
                Status = t.Column<string>(type:"nvarchar(30)", maxLength:30, nullable:false)
            }, constraints: t => { t.PrimaryKey("PK_Billings",x=>x.BillingId); t.ForeignKey("FK_Billings_Sales_SaleId",x=>x.SaleId,"Sales","SaleId",onDelete:ReferentialAction.Restrict); });
            m.CreateIndex(name: "IX_Billings_InvoiceNumber", table: "Billings", column: "InvoiceNumber", unique: true);
            m.CreateIndex(name: "IX_Billings_SaleId", table: "Billings", column: "SaleId", unique: true);
            m.CreateTable("Payments", t => new {
                PaymentId=t.Column<int>(type:"int",nullable:false).Annotation("SqlServer:Identity","1, 1"),
                BillingId=t.Column<int>(type:"int",nullable:false), Amount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),
                Method=t.Column<string>(type:"nvarchar(30)",maxLength:30,nullable:false), Status=t.Column<string>(type:"nvarchar(30)",maxLength:30,nullable:false),
                TransactionReference=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false), PaidAt=t.Column<DateTime>(type:"datetime2",nullable:false)
            }, constraints:t=>{t.PrimaryKey("PK_Payments",x=>x.PaymentId);t.ForeignKey("FK_Payments_Billings_BillingId",x=>x.BillingId,"Billings","BillingId",onDelete:ReferentialAction.Restrict);});
            m.CreateIndex(name: "IX_Payments_BillingId", table: "Payments", column: "BillingId");
        }
        protected override void Down(MigrationBuilder m){m.DropTable("Payments");m.DropTable("Billings");}
    }
}
