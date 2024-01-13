using System;
using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    public class MahasiswaModel
    {
        [Key]
        [Required]
        [StringLength(10)]
        public string nim { get; set; }

        [StringLength(100)]
        public string prodi { get; set; }

        [StringLength(8)]
        public string kelas { get; set; }

        [StringLength(int.MaxValue)]
        public string nama { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string email { get; set; }

        [StringLength(4)]
        public string mhs_angkatan { get; set; }

        [StringLength(int.MaxValue)]
        public string dosen_wali { get; set; }
        public int? status { get; set; } = 1;
        public enum Status
        {
            Aktif,
            TidakAktif
        }
    }
}
