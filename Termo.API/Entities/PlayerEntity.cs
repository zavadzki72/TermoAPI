using System.ComponentModel.DataAnnotations;

namespace Termo.API.Entities {
    public class PlayerEntity {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string IpAdress { get; set; }

    }
}
