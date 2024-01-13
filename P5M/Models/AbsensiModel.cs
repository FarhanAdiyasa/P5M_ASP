using System;
using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    public class AbsensiModel
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string nim { get; set; }
        [Required]
        public DateTime waktu { get; set; }
    }
}
