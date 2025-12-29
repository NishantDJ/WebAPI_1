using System.ComponentModel.DataAnnotations;

namespace WebAPI_1.Model
{
    public class CreateEmployeeDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Department { get; set; } = null!;

        [Range(1000, 1000000)]
        public decimal Salary { get; set; }

        public DateTime DateOfJoining { get; set; }
    }
}
