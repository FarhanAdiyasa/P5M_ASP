using Microsoft.EntityFrameworkCore;

namespace P5M.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PelanggaranModel> Pelanggaran { get; set; }
        public DbSet<PenggunaModel> Pengguna { get; set; }
        public DbSet<MahasiswaModel> Mahasiswa { get; set; }
        public DbSet<P5MModel> P5M { get; set; }
        public DbSet<MahasiswaPelanggaranDataModel> MahasiswaPelanggaranData { get; set; }
        public DbSet<AbsensiViewModel> AbsensiView { get; set; }
        public DbSet<AbsensiModel> Absen { get; set; }
        public DbSet<DetailP5mModel> Detail_p5m { get; set; }
        public DbSet<GetTotalPelanggaranModel> GetTotalPelanggaran { get; set; }
        public DbSet<LogModel> Log { get; set; }
        public DbSet<LiburModel> Libur { get; set; }
    }
}
