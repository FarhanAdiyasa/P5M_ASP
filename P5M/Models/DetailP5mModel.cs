using System;
using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    public class DetailP5mModel
    {
        [Key]
        public int Id { get; set; }
        public int id_p5m { get; set; }
        public int id_pelanggaran { get; set; }
        public int created_by { get; set; }
        public int jam_minus { get; set; }
    }

}
