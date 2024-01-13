using System;
using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    public class LiburModel
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Tanggal harus diisi")]
        public DateTime tanggal { get; set; }

        [StringLength(100, ErrorMessage = "Deskripsi tidak boleh lebih dari 100 karakter")]
        public string deskripsi { get; set; }

        [StringLength(20, ErrorMessage = "Kategori tidak boleh lebih dari 20 karakter")]
        public string kategori { get; set; }

        public int? status { get; set; } = 1;

        public enum Status
        {
            Aktif,
            TidakAktif
        }
    }
}
