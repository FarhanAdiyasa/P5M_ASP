using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace P5M.Models
{
    [Keyless]
    public class AbsensiViewModel
    {
        public string nim { get; set; }
        public string nama { get; set; }
        public string tanggal { get; set; }

        [Column("in")]
        public string inTime { get; set; }

        [Column("out")]
        public string outTime { get; set; }
    }
}
