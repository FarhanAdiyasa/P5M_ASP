using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using P5M.Models;
using System;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace P5M.Controllers
{
    [Authorize]
    public class P5MController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public P5MController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [Authorize]
        public ActionResult Index()
        {
            PelanggaranModel pelanggaranModel = new PelanggaranModel();
            return View(pelanggaranModel);
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListMahasiswa?id_konsentrasi=3";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var dataMahasiswa = JsonConvert.DeserializeObject<List<MahasiswaModel>>(apiResponse);

                    ViewData["KelasMahasiswa"] = dataMahasiswa.Select(dm => dm.kelas).Distinct().ToList();
                }
            }
            var activePelanggaranList = _dbContext.Pelanggaran.Where(m => m.status == 1).ToList();
            return View("../P5M/Create", activePelanggaranList);
        }

        [HttpPost]
        public async Task<ActionResult> Create(List<PelanggaranModel> model)
        {
           
            List<MahasiswaModel> mahasiswaData = await GetMahasiswaFromApi();

            var activePelanggaranList = _dbContext.Pelanggaran.Where(m => m.status == 1).ToList();
            string kelas = "";
            try
            { 
                foreach (var dm in mahasiswaData)
                {
                    foreach (var m in activePelanggaranList)
                    {
                        string key = "CB_" + dm.nim + "_" + m.id;

                        foreach (var formEntry in Request.Form)
                        {
                            if(formEntry.Key == key)
                            {
                                var p5mData = new P5MModel
                                {
                                    nim_mahasiswa = dm.nim,
                                    tgl_transaksi = DateTime.Now,
                                };
                                kelas = dm.kelas;
                                _dbContext.P5M.Add(p5mData);
                                _dbContext.SaveChanges();
                                var detailP5MData = new DetailP5mModel
                                {
                                    id_p5m = p5mData.id_p5m, // Assuming IdP5M is the primary key of P5M
                                    id_pelanggaran = m.id,
                                    jam_minus = m.jam_minus
                                    // Set other properties as needed
                                };

                                _dbContext.Detail_p5m.Add(detailP5MData);
                                _dbContext.SaveChanges();
                               

                            }
                        }
                    }
                }

                AddLog("Menambahkan Pelanggaran P5M Kelas: " + kelas, DateTime.Now);
                TempData["SuccessMessage"] = "Data has been successfully inserted.";
                return RedirectToAction("Create"); 
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                TempData["ErrorMessage"] = "An error occurred while inserting data.";
                return RedirectToAction("Create"); 
            }
        }
        [HttpGet]
        public async Task<ActionResult> HistoryP5M()
        {
            if (HttpContext.Session.GetString("LoggedInRole") == "KOORDINATOR SOP dan TATIB" || HttpContext.Session.GetString("LoggedInRole") == "SEKRETARIS PRODI")
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = "https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListMahasiswa?id_konsentrasi=3";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        var dataMahasiswa = JsonConvert.DeserializeObject<List<MahasiswaModel>>(apiResponse);

                        ViewData["KelasMahasiswa"] = dataMahasiswa.Select(dm => dm.kelas).Distinct().ToList();
                    }
                }
                return View("../P5M/Pilih_Kelas_History");
            }
            else
            {
                // Redirect to HistoryP5MKelas with the logged-in user's class
                return RedirectToAction("HistoryP5MKelas", new { kelas = HttpContext.Session.GetString("LoggedInKelas") });
            }
        }

        [HttpGet]
        public ActionResult HistoryP5MKelas(string kelas)
        {
            ViewData["Kelas"] = kelas;
            var activePelanggaranList = _dbContext.Pelanggaran.Where(m => m.status == 1).ToList();
            return View("../P5M/History", activePelanggaranList);
        }

        [HttpGet]
        public ActionResult CheckRecordExistence(string nim, int pelanggaranId, DateTime tanggal)
        {
            // Extract the date component from the provided DateTime
            DateTime tanggalWithoutTime = tanggal.Date;

            Console.WriteLine(tanggalWithoutTime.ToString());

            var exists = _dbContext.Detail_p5m.Any(d =>
                _dbContext.P5M.Any(p => p.nim_mahasiswa == nim && p.id_p5m == d.id_p5m && p.tgl_transaksi.Date == tanggalWithoutTime) &&
                d.id_pelanggaran == pelanggaranId
            );

            return Json(new { exists = exists });
        }

        private async Task<List<MahasiswaModel>> GetMahasiswaFromApi()
        {
            string apiUrl = "https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListMahasiswa?id_konsentrasi=3";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    List<MahasiswaModel> mahasiswas = JsonConvert.DeserializeObject<List<MahasiswaModel>>(apiResponse);
                    return mahasiswas;
                }
                else
                {
                    // Handle error jika ada
                    return new List<MahasiswaModel>();
                }
            }
        }

        public ActionResult Lihat()
        {
            PelanggaranModel pelanggaranModel = new PelanggaranModel();
            return View(pelanggaranModel);
        }

        public async Task<IActionResult> History()
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListMahasiswa?id_konsentrasi=3";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var dataMahasiswa = JsonConvert.DeserializeObject<List<MahasiswaModel>>(apiResponse);

                    ViewData["KelasMahasiswa"] = dataMahasiswa.Select(dm => dm.kelas).Distinct().ToList();
                }
            }
            PelanggaranModel pelanggaranModel = new PelanggaranModel();
            return View("../Laporan/HistoryP5M", pelanggaranModel);
        }
        public async Task<IActionResult> HistoryAbsen()
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListMahasiswa?id_konsentrasi=3";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var dataMahasiswa = JsonConvert.DeserializeObject<List<MahasiswaModel>>(apiResponse);

                    ViewData["KelasMahasiswa"] = dataMahasiswa.Select(dm => dm.kelas).Distinct().ToList();
                }
            }
            PelanggaranModel pelanggaranModel = new PelanggaranModel();
            return View("../Laporan/HistoryAbsen", pelanggaranModel);
        }
        public async Task<IActionResult> HistoryAbsensi()
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListMahasiswa?id_konsentrasi=3";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var dataMahasiswa = JsonConvert.DeserializeObject<List<MahasiswaModel>>(apiResponse);

                    ViewData["KelasMahasiswa"] = dataMahasiswa.Select(dm => dm.kelas).Distinct().ToList();
                }
            }
            PelanggaranModel pelanggaranModel = new PelanggaranModel();
            return View("../Laporan/HistoryAbsensi", pelanggaranModel);
        }
        public async Task<IActionResult> LoadPartialViewAbsensi(string filterValue, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<AbsensiViewModel> result = new List<AbsensiViewModel>();

                // Get data from the API
                List<MahasiswaModel> dataMahasiswa = await GetMahasiswaFromApi();
                // Query to get absensi data
                var absensiResult = _dbContext.Absen
                    .AsEnumerable() 
                    .Join(
                        inner: dataMahasiswa,
                        outerKeySelector: a => a.nim,
                        innerKeySelector: m => m.nim,
                        resultSelector: (a, m) => new
                        {
                            a = a,
                            m = m
                        })
                    .Where(x => x.m.kelas == filterValue && x.a.waktu >= startDate && x.a.waktu <= endDate.AddDays(1))
                    .GroupBy(x => new { x.a.nim, x.m.nama, Date = x.a.waktu.Date })
                    .Select(g => new
                    {
                        nim = g.Key.nim,
                        nama = g.Key.nama,
                        tanggal = g.Key.Date.ToString("dd-MM-yyyy"),
                        inTime = g.Min(x => x.a.waktu).ToString("HH:mm:ss"),
                        outTime = g.Max(x => x.a.waktu).ToString("HH:mm:ss")
                    });

                foreach (var item in absensiResult)
                {
                    AbsensiViewModel absensiData = new AbsensiViewModel
                    {
                        nim = item.nim,
                        nama = item.nama,
                        tanggal = item.tanggal,
                        inTime = item.inTime,
                        outTime = item.outTime
                    };

                    result.Add(absensiData);
                }

                ViewData["kelas"] = filterValue;
                ViewData["tanggalMulai"] = startDate;
                ViewData["tanggalSelesai"] = endDate;
                ViewData["nimarray"] = dataMahasiswa
         .Where(dm => dm.kelas == filterValue)
         .Select(dm => dm.nim)
         .ToList();
            ViewData["namaarray"] = dataMahasiswa
         .Where(dm => dm.kelas == filterValue)
         .Select(dm => dm.nama)
         .ToList();
                ViewData["tanggalarray"] = _dbContext.Libur.Where(dm=> dm.tanggal >= startDate && dm.tanggal <= endDate.AddDays(1))
     .Select(dm => dm.tanggal.ToString("dd-MM-yyyy"))
     .ToList();
                ViewData["namaLiburarray"] = _dbContext.Libur.Where(dm => dm.tanggal >= startDate && dm.tanggal <= endDate.AddDays(1))
    .Select(dm => dm.deskripsi)
    .ToList();



                return PartialView("../Laporan/_AbsensiPartialView", result);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }
        public async Task<IActionResult> LoadPartialView(string filterValue, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Mengambil data dari API
                List<MahasiswaModel> mahasiswas = await GetMahasiswaFromApi();

                // Logika untuk menggabungkan data dari API dengan data dari database
                var result = mahasiswas
                    .Join(_dbContext.P5M, apiMahasiswa => apiMahasiswa.nim, p5m => p5m.nim_mahasiswa, (apiMahasiswa, p5m) => new { apiMahasiswa, p5m })
                    .Join(_dbContext.Detail_p5m, x => x.p5m.id_p5m, dp => dp.id_p5m, (x, dp) => new { x.apiMahasiswa, x.p5m, dp })
                    .Where(x => x.p5m.tgl_transaksi >= startDate && x.p5m.tgl_transaksi <= endDate.AddDays(1) && x.apiMahasiswa.kelas == filterValue)
                    .GroupBy(x => new { x.apiMahasiswa.nim, x.apiMahasiswa.nama })
                    .Select(g => new P5M.Models.MahasiswaPelanggaranDataModel
                    {
                        nim = g.Key.nim,
                        nama = g.Key.nama,
                        jenis = "Murni",
                        jumlah_jam = g.Sum(x => x.dp.jam_minus != null ? x.dp.jam_minus : 0),
                        keterangan = $"Jam Minus P5M Prodi MI Periode {startDate.ToString("dd-MM-yyyy")} - {endDate.ToString("dd-MM-yyyy")}",
                        tanggal = DateTime.Now
                    })
                    .ToList();

                if (result == null)
                {
                    Console.WriteLine("Result is null");
                }
                return PartialView("../Laporan/_P5MPartialView", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw; // Rethrow the exception to capture detailed error information
            }
        }
        public async Task<IActionResult> LoadPartialViewAbsen(string filterValue, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<AbsensiViewModel> result = new List<AbsensiViewModel>();

                // Get data from the API
                List<MahasiswaModel> dataMahasiswa = await GetMahasiswaFromApi();

                // Query to get absensi data
                var absensiResult = _dbContext.Absen
                    .AsEnumerable()
                    .Join(
                        inner: dataMahasiswa,
                        outerKeySelector: a => a.nim,
                        innerKeySelector: m => m.nim,
                        resultSelector: (a, m) => new
                        {
                            a = a,
                            m = m
                        })
                    .Where(x => x.m.kelas == filterValue && x.a.waktu >= startDate && x.a.waktu <= endDate.AddDays(1))
                    .GroupBy(x => new { x.a.nim, x.m.nama, Date = x.a.waktu.Date })
                    .Select(g => new
                    {
                        nim = g.Key.nim,
                        nama = g.Key.nama,
                        tanggal = g.Key.Date.ToString("dd-MM-yyyy"),
                        inTime = g.Min(x => x.a.waktu).ToString("HH:mm:ss"),
                        outTime = g.Max(x => x.a.waktu).ToString("HH:mm:ss")
                    });

                foreach (var item in absensiResult)
                {
                    AbsensiViewModel absensiData = new AbsensiViewModel
                    {
                        nim = item.nim,
                        nama = item.nama,
                        tanggal = item.tanggal,
                        inTime = item.inTime,
                        outTime = item.outTime
                    };

                    result.Add(absensiData);
                }

                ViewData["kelas"] = filterValue;
                ViewData["tanggalMulai"] = startDate;
                ViewData["tanggalSelesai"] = endDate;
                ViewData["nimarray"] = dataMahasiswa
         .Where(dm => dm.kelas == filterValue)
         .Select(dm => dm.nim)
         .ToList();
                ViewData["namaarray"] = dataMahasiswa
         .Where(dm => dm.kelas == filterValue)
         .Select(dm => dm.nama)
         .ToList();
                ViewData["tanggalarray"] = _dbContext.Libur
       .Where(dm => dm.tanggal >= startDate && dm.tanggal <= endDate.AddDays(1))
       .Select(dm => dm.tanggal.ToString("dd-MM-yyyy"))
       .ToList();

                ViewData["namaLiburarray"] = _dbContext.Libur.Where(dm => dm.tanggal >= startDate && dm.tanggal <= endDate.AddDays(1))
    .Select(dm => dm.deskripsi)
    .ToList();

                return PartialView("../Laporan/_AbsenPartialView", result);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }



        private int HitungJamMinus(string inTime, string outTime)
        {
            // Konversi string waktu ke objek DateTime
            DateTime inTimeDateTime = DateTime.ParseExact(inTime, "HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime outTimeDateTime = DateTime.ParseExact(outTime, "HH:mm:ss", CultureInfo.InvariantCulture);

            // Tentukan batas waktu in dan out
            DateTime batasInTime = DateTime.ParseExact("07:30:00", "HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime batasOutTime = DateTime.ParseExact("16:30:00", "HH:mm:ss", CultureInfo.InvariantCulture);

            // Hitung selisih waktu
            TimeSpan selisihInTime = inTimeDateTime - batasInTime;
            TimeSpan selisihOutTime = batasOutTime - outTimeDateTime;

            // Hitung jam minus
            int jamMinus = 0;

            Console.WriteLine("Selisih In: " + selisihInTime.TotalMinutes);
            if (selisihInTime.TotalMinutes < 30 && selisihInTime.TotalMinutes > 0 && inTimeDateTime.TimeOfDay < new TimeSpan(12, 0, 0))
            {
                jamMinus += 1;
                Console.WriteLine(inTimeDateTime);
            }
            else if (selisihInTime.TotalMinutes > 30 && inTimeDateTime.TimeOfDay < new TimeSpan(12, 0, 0))
            {
                jamMinus += (int)Math.Ceiling(selisihInTime.TotalHours) * 2;
                Console.WriteLine((int)Math.Ceiling(selisihInTime.TotalHours) * 2);
                Console.WriteLine("Lebih");
            }else if(inTimeDateTime.TimeOfDay > new TimeSpan(12, 0, 0))
            {
                jamMinus += 4;
            }

            Console.WriteLine("Selisih Out: " + selisihOutTime.TotalMinutes);
            if (selisihOutTime.TotalMinutes < 30 && selisihOutTime.TotalMinutes > 0 && outTimeDateTime.TimeOfDay > new TimeSpan(12, 0, 0))
            {
                jamMinus += 1;
                Console.WriteLine(outTimeDateTime);
            }
            else if (selisihOutTime.TotalMinutes > 30 && outTimeDateTime.TimeOfDay > new TimeSpan(12, 0, 0))
            {
                jamMinus += (int)Math.Ceiling(selisihOutTime.TotalHours) * 2;
                Console.WriteLine((int)Math.Ceiling(selisihOutTime.TotalHours) * 2);
                Console.WriteLine("Lebih");
            }
            else if (outTimeDateTime.TimeOfDay < new TimeSpan(12, 0, 0))
            {
                jamMinus += 4;
            }

            return jamMinus;
        }
        private void AddLog(string aktifitas, DateTime tanggal)
        {
            var loggedInUsername = HttpContext.Session.GetString("LoggedInUsername");

            var log = new LogModel
            {
                aktifitas = loggedInUsername + ", " + aktifitas,
                tanggal = tanggal
            };

            _dbContext.Log.Add(log);
            _dbContext.SaveChanges();
        }
    }
}
