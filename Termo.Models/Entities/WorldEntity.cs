using System;
using System.ComponentModel.DataAnnotations;
using Termo.Models.Enumerators;

namespace Termo.Models.Entities {
    public class WorldEntity {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public WorldStatusEnumerator WorldStatus { get; set; }
        public DateTime? UsedDate { get; set; }

    }
}
