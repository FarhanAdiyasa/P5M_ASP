using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    public class PelanggaranModel
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Nama pelanggaran wajib diisi!")]
        [MaxLength(100, ErrorMessage = "Nama pelanggaran maksimal 100 karakter")]
        public string nama_pelanggaran { get; set; }

        [Required(ErrorMessage = "Jam minus wajib diisi!")]
        public int jam_minus { get; set; }

        public int? status { get; set; } = 1;
        public enum Status
        {
            Aktif,
            TidakAktif
        }

    }
}
