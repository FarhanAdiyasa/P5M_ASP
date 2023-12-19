using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using P5M.Models;
using System;
using System.Linq;
using System.Data.SqlClient;


namespace P5M.Controllers
{
    public class P5MController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public P5MController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public ActionResult Index()
        {
            PelanggaranModel pelanggaranModel = new PelanggaranModel();
            return View(pelanggaranModel);
        }

        public async Task<ActionResult> Create()
        {
            List<MahasiswaModel> data = await GetDataFromAPI();
            return View(data);
        }

        private async Task<List<MahasiswaModel>> GetDataFromAPI()
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetStringAsync("https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListMahasiswa?id_konsentrasi=3");
                return JsonConvert.DeserializeObject<List<MahasiswaModel>>(result);
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
        public IActionResult LoadPartialView(string filterValue, DateTime startDate, DateTime endDate)
        {
            List<MahasiswaPelanggaranDataModel> result = new List<MahasiswaPelanggaranDataModel>();
            string connectionString = "Server=CLIENTINTERNAL\\MSSQLSERVER02;Database=fingerspot;Integrated Security=true;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM GetMahasiswaPelanggaranByDateRange(@kelas, @startdate, @enddate)", connection))
                {
                    command.Parameters.AddWithValue("@kelas", filterValue);
                    command.Parameters.AddWithValue("@startdate", startDate);
                    command.Parameters.AddWithValue("@enddate", endDate);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MahasiswaPelanggaranDataModel mahasiswa = new MahasiswaPelanggaranDataModel
                            {
                                nim = reader["nim"].ToString(),
                                nama = reader["nama"].ToString(),
                                jenis = reader["jenis"].ToString(),
                                jumlah_jam = Convert.ToInt32(reader["jumlah_jam"]),
                                keterangan = reader["keterangan"].ToString(),
                                tanggal = Convert.ToDateTime(reader["tanggal"]),
                            };

                            result.Add(mahasiswa);
                        }
                    }
                }
            }
            return PartialView("../Laporan/_P5MPartialView", result);
        }
        public IActionResult LoadPartialViewAbsen(string filterValue, DateTime startDate, DateTime endDate)
        {
            List<MahasiswaPelanggaranDataModel> result = new List<MahasiswaPelanggaranDataModel>();
            string connectionString = "Server=CLIENTINTERNAL\\MSSQLSERVER02;Database=fingerspot;Integrated Security=true;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM GetAbsenSummaryByDateRangeWithDetails(@kelas, @startdate, @enddate)", connection))
                {
                    command.Parameters.AddWithValue("@kelas", filterValue);
                    command.Parameters.AddWithValue("@startdate", startDate);
                    command.Parameters.AddWithValue("@enddate", endDate);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MahasiswaPelanggaranDataModel mahasiswa = new MahasiswaPelanggaranDataModel
                            {
                                nim = reader["nim"].ToString(),
                                nama = reader["nama"].ToString(),
                                jenis = reader["jenis"].ToString(),
                                jumlah_jam = Convert.ToInt32(reader["jumlah_jam"]),
                                keterangan = reader["keterangan"].ToString(),
                                tanggal = Convert.ToDateTime(reader["tanggal"]),
                            };

                            result.Add(mahasiswa);
                        }
                    }
                }
            }
            return PartialView("../Laporan/_AbsenPartialView", result);
        }
        public IActionResult LoadPartialViewAbsensi(string filterValue, DateTime startDate, DateTime endDate)
        {
            List<AbsensiViewModel> result = new List<AbsensiViewModel>();
            string connectionString = "Server=CLIENTINTERNAL\\MSSQLSERVER02;Database=fingerspot;Integrated Security=true;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT * FROM GetAbsenViewByDateRange(@kelas, @startdate, @enddate)", connection))
                {
                    command.Parameters.AddWithValue("@kelas", filterValue);
                    command.Parameters.AddWithValue("@startdate", startDate);
                    command.Parameters.AddWithValue("@enddate", endDate);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AbsensiViewModel mahasiswa = new AbsensiViewModel
                            {
                                nim = reader["nim"].ToString(),
                                nama = reader["nama"].ToString(),
                                tanggal = reader["tanggal"].ToString(),
                                inTime = reader["in"].ToString(),
                                outTime = reader["out"].ToString()
                            };

                            result.Add(mahasiswa);
                        }
                    }
                }
            }
            ViewData["kelas"] = filterValue;
            ViewData["tanggalMulai"] = startDate;
            ViewData["tanggalSelesai"] = endDate;
            return PartialView("../Laporan/_AbsensiPartialView", result);
        }
    }
}
