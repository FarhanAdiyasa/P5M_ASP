using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using P5M.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;

namespace P5M.Controllers
{
    [Authorize]
    public class MahasiswaController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public MahasiswaController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            List<MahasiswaModel> mahasiswaList = null; // Corrected the type

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListMahasiswa?id_konsentrasi=3";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    mahasiswaList = JsonConvert.DeserializeObject<List<MahasiswaModel>>(apiResponse);
                    ViewData["Mahasiswa"] = mahasiswaList;
                }
            }

            return View(mahasiswaList);
        }



        [HttpGet]
        public async Task<IActionResult> Create()
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListKonsentrasi";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var dataArray = JsonConvert.DeserializeObject<List<JObject>>(apiResponse);
                    ViewData["KelasMahasiswaNama"] = dataArray.Select(dm => dm.Value<string>("kon_nama")).ToList();
                }
            }

            return View();
        }


        [HttpPost]
        public IActionResult Create(MahasiswaModel mahasiswaModel)
        {

            if (ModelState.IsValid)
            {
                _dbContext.Mahasiswa.Add(mahasiswaModel);
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Data berhasil ditambahkan";
                return RedirectToAction("Index");
            }

            return View(mahasiswaModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string nim)
        {

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://api.polytechnic.astra.ac.id:2906/api_dev/efcc359990d14328fda74beb65088ef9660ca17e/SIA/getListKonsentrasi";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var dataArray = JsonConvert.DeserializeObject<List<JObject>>(apiResponse);
                    ViewData["KelasMahasiswaNama"] = dataArray.Select(dm => dm.Value<string>("kon_nama")).ToList();
                }
            }
            MahasiswaModel mahasiswaModel = _dbContext.Mahasiswa.Find(nim);
            if (mahasiswaModel == null)
            {
                return NotFound();
            }
            return View(mahasiswaModel);
        }

        [HttpPost]
        public IActionResult Edit(MahasiswaModel mahasiswaModel)
        {
            if (ModelState.IsValid)
            {
                MahasiswaModel existingMahasiswaModel = _dbContext.Mahasiswa.Find(mahasiswaModel.nim);

                if (existingMahasiswaModel == null)
                {
                    return NotFound();
                }

                existingMahasiswaModel.prodi = mahasiswaModel.prodi;
                existingMahasiswaModel.kelas = mahasiswaModel.kelas;
                existingMahasiswaModel.nama = mahasiswaModel.nama;
                existingMahasiswaModel.email = mahasiswaModel.email;
                existingMahasiswaModel.mhs_angkatan = mahasiswaModel.mhs_angkatan;
                existingMahasiswaModel.dosen_wali = mahasiswaModel.dosen_wali;
                existingMahasiswaModel.status = mahasiswaModel.status;

                _dbContext.Mahasiswa.Update(existingMahasiswaModel);
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Data Mahasiswa berhasil diupdate.";
                return RedirectToAction("Index");
            }
            return View(mahasiswaModel);
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            var response = new { success = false, message = "Gagal menghapus Data Mahasiswa." };
            try
            {
                MahasiswaModel penggunaModel = _dbContext.Mahasiswa.Find(id);
                if (penggunaModel != null)
                {
                    // Instead of directly removing, update the status to 0
                    penggunaModel.status = 0;
                    _dbContext.Mahasiswa.Update(penggunaModel);
                    _dbContext.SaveChanges();

                    response = new { success = true, message = "Data Mahasiswa berhasil dihapus." };
                }
                else
                {
                    response = new { success = false, message = "Data Mahasiswa tidak ditemukan." };
                }
            }
            catch (Exception ex)
            {
                response = new { success = false, message = ex.Message };
            }
            return Json(response);
        }
    }
}
