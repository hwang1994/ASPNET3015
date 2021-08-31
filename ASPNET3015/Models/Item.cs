using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPNET3015.Models
{
    public class Item : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        [RegularExpression(@"^[a-zA-Z0-9'-]{1,40}$", ErrorMessage = "Invalid Title")]
        public string Title { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]+[.][0-9]{2}$", ErrorMessage = "Invalid Price")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public string Picture { get; set; }

        [Required]
        [ForeignKey("Id")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public List<Pin> Pins { get; set; }
        public List<Downvote> Downvotes { get; set; }
    }
}