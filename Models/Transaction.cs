using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccessibleBank.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public int FromAccountId { get; set; }

        [Required]
        public int ToAccountId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [ForeignKey("FromAccountId")]
        public Account? FromAccount { get; set; }

        [ForeignKey("ToAccountId")]
        public Account? ToAccount { get; set; }
    }
}
