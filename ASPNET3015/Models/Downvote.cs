using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPNET3015.Models
{
    public class Downvote : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Id")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [ForeignKey("Id")]
        public int ItemId { get; set; }
        public Item Item { get; set; }
    }
}