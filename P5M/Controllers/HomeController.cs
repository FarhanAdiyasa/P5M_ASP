 using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using System.Diagnostics;
using System.Text;
using P5M.Models;
using System.Reflection.PortableExecutable;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Authorization;

namespace P5M.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private static string importStatus = string.Empty;
        private static readonly object lockObject = new object();

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment, IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _environment = environment;
            _configuration = configuration;
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            return View("../ImportAbsen/Index");
        }
        public IActionResult AccesDenied()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            var latestAbsen = _dbContext.Absen
                .OrderByDescending(a => a.waktu)
                .FirstOrDefault();
            if (latestAbsen != null)
            {
                var latestWaktu = latestAbsen.waktu.Date.ToString("dd-MM-yyyy") ;
                ViewData["LatestWaktu"] = latestWaktu;
            }
            else
            {
                ViewData["LatestWaktu"] = null;
            }
            var logs = _dbContext.Log.ToList();

            var today = DateTime.Today;
            var sudahP5M = _dbContext.P5M
                .Any(p => p.tgl_transaksi.Date == today);

            ViewData["SudahP5M"] = sudahP5M;
            return View(logs);
        }
        public IActionResult LoadChart(DateTime startDate, DateTime endDate)
        {
            // Truncate time part from startDate and endDate
            startDate = startDate.Date;
            endDate = endDate.Date;

            var result = GetTotalPelanggaranData(startDate, endDate);
            return PartialView("_chartPelanggaran", result);
        }

        private List<GetTotalPelanggaranModel> GetTotalPelanggaranData(DateTime startDate, DateTime endDate)
        {
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59);
            var result = _dbContext.GetTotalPelanggaran
                .FromSqlRaw("SELECT * FROM dbo.GetTotalPelanggaran({0}, {1})", startDate, endDate.AddDays(1))
                .ToList();
            Console.WriteLine(startDate.ToString() + endDate.ToString());

            // Create a new list to store the calculated results
            List<GetTotalPelanggaranModel> calculatedResults = new List<GetTotalPelanggaranModel>();
            
            // Calculate total pelanggaran
            foreach (var item in result)
            {
                calculatedResults.Add(new GetTotalPelanggaranModel
                {
                    nama_pelanggaran = item.nama_pelanggaran,
                    total_pelanggaran_dilakukan = item.total_pelanggaran_dilakukan
                });
            }
            Console.WriteLine(calculatedResults.Count());

            return calculatedResults;
        }
        public IActionResult LoadChartNim(DateTime startDate, DateTime endDate)
        {
            // Truncate time part from startDate and endDate
            startDate = startDate.Date;
            endDate = endDate.Date;

            if (startDate == default(DateTime) || endDate == default(DateTime))
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            var result = GetNimPelanggaranData(startDate, endDate);
            return PartialView("_chartNimPelanggaran", result);
        }
        private List<GetTotalPelanggaranModel> GetNimPelanggaranData(DateTime startDate, DateTime endDate)
        {
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59);
            var result = _dbContext.GetTotalPelanggaran
                .FromSqlRaw("SELECT TOP 5 * FROM dbo.GetNimPelanggaran({0}, {1})", startDate, endDate.AddDays(1))
                .ToList();

            // Create a new list to store the calculated results
            List<GetTotalPelanggaranModel> calculatedResults = new List<GetTotalPelanggaranModel>();

            // Calculate total pelanggaran
            foreach (var item in result)
            {
                calculatedResults.Add(new GetTotalPelanggaranModel
                {
                    nama_pelanggaran = item.nama_pelanggaran,
                    total_pelanggaran_dilakukan = item.total_pelanggaran_dilakukan
                });
            }

            return calculatedResults;
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            int insert = 0;
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                if (file != null && file.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "Uploads");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    int totalRecords = GetTotalRows(filePath);

                    using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            bool isHeaderSkipped = false;

                            while (reader.Read())
                            {
                                if (!isHeaderSkipped)
                                {
                                    isHeaderSkipped = true;
                                    continue;
                                }

                                AbsensiModel s = new AbsensiModel();
                                s.nim = reader.GetValue(3).ToString();
                                string waktuString = reader.GetValue(0).ToString();
                                s.waktu = DateTime.TryParse(waktuString, out DateTime waktu) ? waktu : default;

                                if (!_dbContext.Absen.Any(a => a.nim == s.nim && a.waktu == s.waktu))
                                {
                                    _dbContext.Add(s);
                                    await _dbContext.SaveChangesAsync();
                                    UpdateImportStatus($"Data added: NIM = {s.nim}, Waktu = {s.waktu}", reader.Depth, totalRecords);
                                    insert++;
                                }
                                else
                                {
                                    UpdateImportStatus($"Data already exists: NIM = {s.nim}, Waktu = {s.waktu}", reader.Depth, totalRecords);
                                }
                            }
                        } while (reader.NextResult());
                    }

                    UpdateImportStatus("success inserting ", insert, totalRecords);
                    AddLog("Import Absen : " + file.FileName, DateTime.Now);
                    System.IO.File.Delete(filePath);
                }
                else
                {
                    ViewBag.Message = "empty";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                _logger.LogError(ex, "Error during file processing.");
            }

            return View("../ImportAbsen/Index");
        }

        private void UpdateImportStatus(string status, int currentRecord, int totalRecords)
        {
            lock (lockObject)
            {
                importStatus = $"{status} ({currentRecord} out of {totalRecords} records)";
            }
        }

        public string GetImportStatus()
        {
            lock (lockObject)
            {
                return importStatus;
            }
        }

        private int GetTotalRows(string filePath)
        {
            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                int totalRows = 0;
                do
                {
                    while (reader.Read())
                    {
                        totalRows++;
                    }
                } while (reader.NextResult());

                return totalRows;
            }
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