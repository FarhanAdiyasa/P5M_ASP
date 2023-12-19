using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    public class PenggunaModel
    {
        [Key]
        public int id { get; set; }

        [MaxLength(50, ErrorMessage = "Username maksimal 50 karakter")]
        public string username { get; set; }

        [Required(ErrorMessage = "Nama pengguna wajib diisi!")]
        [MaxLength(100, ErrorMessage = "Nama pengguna maksimal 100 karakter")]
        public string nama_pengguna { get; set; }

        [Required(ErrorMessage = "Role wajib diisi!")]
        [MaxLength(50, ErrorMessage = "Role maksimal 50 karakter")]
        public string role { get; set; }

        [Required(ErrorMessage = "Kelas wajib diisi!")]
        [MaxLength(15, ErrorMessage = "Kelas maksimal 15 karakter")]
        public string kelas { get; set; }

        public int? status { get; set; } = 1; // Set default value to 1

        public enum Status
        {
            Aktif = 1,
            TidakAktif = 0
        }
    }
}
