# Invenzo — Billing, Payment & ZenoAI update

## Sales -> Billing -> Payment workflow

1. Seller creates a Sale.
2. The SaleService automatically creates one invoice in Billing.
3. Billing immediately shows the new invoice as **Outstanding**.
4. A **Pending** payment record is created automatically.
5. Seller opens **Invoice / Payment**, selects UPI, Card, Net Banking or Cash and clicks **Pay & Mark Paid**.
6. PaymentController marks the pending payment as **Paid** and the invoice as **Paid**.
7. Billing totals therefore update automatically: Total Invoices, Paid and Outstanding.
8. A paid sale is protected from editing so payment history is not silently corrupted.
9. Deleting a sale removes its invoice/payment records and restores stock through the existing SaleService workflow.

Purchases do **not** create customer billing invoices; Billing is tied to sales, as requested.

## ZenoAI

ZenoAI works with the live Invenzo database and can answer:
- current stock
- low-stock/reorder items
- sales/revenue
- billing totals (invoices, paid, outstanding)
- recorded payments

It also supports report generation:
- "Generate sales report"
- "Generate purchase report"
- "Generate inventory report"

The generated reports are downloadable CSV files from ZenoAI.

## Database

A migration named `AddBillingAndPayments` creates `Billings` and `Payments`. The application applies migrations at startup.

## Note about real payments

The payment flow is a working **internal payment recording workflow**. It does not move real money. A real UPI/card gateway such as Razorpay or Stripe requires merchant credentials and gateway callbacks/webhooks.
