using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccessibleBank.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; } //making the property nullable with ?

        public decimal Balance { get; set; }

        public string Currency { get; set; } = "AED";
    }
}
