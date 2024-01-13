using System;
using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    public class P5MModel
    {
        [Key]
        public int id_p5m { get; set; }

        [Required(ErrorMessage = "NIM Mahasiswa wajib diisi!")]
        [StringLength(12, ErrorMessage = "NIM Mahasiswa maksimal 12 karakter")]
        public string nim_mahasiswa { get; set; }

        [Required(ErrorMessage = "Tanggal transaksi wajib diisi!")]
        [DataType(DataType.DateTime)]
        public DateTime tgl_transaksi { get; set; }
    }
}
