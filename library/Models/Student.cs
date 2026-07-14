using System.ComponentModel.DataAnnotations.Schema;

namespace library.Models
{
    public class Student
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Faculty { get; set; }
        public string MajorName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        [ForeignKey("User")]
        public long? UserId { get; set; }
        public User? User { get; set; }
    }
}
