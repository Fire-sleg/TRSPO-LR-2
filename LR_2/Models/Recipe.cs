using System.ComponentModel.DataAnnotations;

namespace LR_2.Models
{
    public class Recipe
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;

        public string Description { get; set; } = default!;
        public string ImageURL { get; set; } = default!;

    }
}
