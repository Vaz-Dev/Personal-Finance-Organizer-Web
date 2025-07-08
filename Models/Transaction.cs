namespace PFO_Web.Models
{
    public class Transaction
    {

        public string Id { get; set; }

        public decimal Amount { get; set; }

        public DateOnly Date { get; set; }

        public string? Meta { get; set; }

        public Category? Category { get; set; }


        // to do: add services logic to require PaymentType if this Transaction's Category is a Expense, but not Income.
        public PaymentType? PaymentType { get; set; }
    }
}
