using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Termo.Models.Entities {
    public class AttemptEntity {

        [Key]
        public int Id { get; set; }
        
        public DateTime AttemptDate { get; set; }
        public bool Success { get; set; }
        public string TriedWorld { get; set; }
        public int AttemptNumber { get; set; }
        public int AttemptDay { get; set; }
        public int PlayerId { get; set; }

        [Column("AttemptTry", TypeName = "jsonb")]
        public string AttemptTry { get; set; }


        public virtual PlayerEntity Player { get; set; }
    }
}
