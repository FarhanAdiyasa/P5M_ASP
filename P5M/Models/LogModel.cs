using System;
using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    public class LogModel
    {
        [Key]
        public int id { get; set; }
        public string aktifitas { get; set; }
        public DateTime tanggal { get; set; }
    }
}
