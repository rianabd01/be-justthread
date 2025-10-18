using System;

namespace be_justthread.Models
{
    public class Thread
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
