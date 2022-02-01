using System;
using System.ComponentModel.DataAnnotations;

namespace Termo.Models.Entities
{
    public class InvalidWorldEntity
    {
        [Key]
        public int Id { get; set; }
        public string World { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
