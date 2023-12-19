﻿using Microsoft.EntityFrameworkCore;

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
    }
}