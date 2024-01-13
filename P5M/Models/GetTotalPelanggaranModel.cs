using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace P5M.Models
{
    [Keyless]
    public class GetTotalPelanggaranModel
    {
        public string nama_pelanggaran { get; set; }
        public int total_pelanggaran_dilakukan { get; set; }
    }
}
