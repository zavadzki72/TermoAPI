using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Termo.Models.Entities {
    public class TryEntity {

        [Key]
        public int Id { get; set; }
        
        public DateTime TryDate { get; set; }
        public bool Success { get; set; }
        public int PlayerId { get; set; }

        [Column("JsonTry", TypeName = "jsonb")]
        public string JsonTry { get; set; }

        public string TriedWorld { get; set; }

        public virtual PlayerEntity Player { get; set; }
    }
}
