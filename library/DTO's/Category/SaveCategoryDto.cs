using System.ComponentModel.DataAnnotations;

namespace library.DTO_s.Category
{
    public class SaveCategoryDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }
    }
}
