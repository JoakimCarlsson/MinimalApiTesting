using System.ComponentModel.DataAnnotations;

namespace MinimalApiTesting.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        public bool IsCompleted { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}
