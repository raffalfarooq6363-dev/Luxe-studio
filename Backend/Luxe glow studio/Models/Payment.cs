using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public int AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DiscountAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TaxAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TipAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Stripe, Razorpay, UPI, etc.

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded, Cancelled

        // Payment gateway specific fields
        public string? StripePaymentIntentId { get; set; }

        public string? StripeChargeId { get; set; }

        public string? RazorpayPaymentId { get; set; }

        public string? RazorpayOrderId { get; set; }

        public string? TransactionId { get; set; }

        public string? GatewayResponse { get; set; } // JSON response from payment gateway

        // Payment timing
        public DateTime? PaymentDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Refund information
        public bool IsRefunded { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? RefundAmount { get; set; }

        public string? RefundReason { get; set; }

        public DateTime? RefundDate { get; set; }

        public string? RefundTransactionId { get; set; }

        // Offer/Discount applied
        public int? OfferCodeId { get; set; }
        public virtual OfferCode? OfferCode { get; set; }

        [StringLength(50)]
        public string? CouponCode { get; set; }

        // Receipt information
        public string? ReceiptNumber { get; set; }

        public string? ReceiptUrl { get; set; }

        // Notes
        public string? Notes { get; set; }

        public string? FailureReason { get; set; }

        // Computed properties
        [NotMapped]
        public decimal NetAmount => TotalAmount - (RefundAmount ?? 0);

        [NotMapped]
        public bool IsCompleted => Status == "Completed";

        [NotMapped]
        public bool IsPending => Status == "Pending";

        [NotMapped]
        public bool IsFailed => Status == "Failed";
    }
}