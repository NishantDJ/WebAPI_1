namespace WebAPI_1.Model
{
    public class UpdateEmployeeDto
    {
        public string Name { get; set; } = null!;
        public string Department { get; set; } = null!;
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
    }

}