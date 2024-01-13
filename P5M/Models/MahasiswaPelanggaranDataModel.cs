using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace P5M.Models
{
    [Keyless]
    public class MahasiswaPelanggaranDataModel
    {
        public string nim { get; set; }
        public string nama { get; set; }
        public string jenis { get; set; }
        public int jumlah_jam { get; set; }
        public string keterangan { get; set; }
        public DateTime tanggal { get; set; }
    }

}
