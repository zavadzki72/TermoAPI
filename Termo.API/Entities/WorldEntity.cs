using System;
using System.ComponentModel.DataAnnotations;
using Termo.API.Models;

namespace Termo.API.Entities {
    public class WorldEntity {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public WorldStatusEnumerator WorldStatus { get; set; }
        public DateTime? UsedDate { get; set; }

    }
}
