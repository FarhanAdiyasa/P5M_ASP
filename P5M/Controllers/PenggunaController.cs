using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using P5M.Models;
using System;
using System.Linq;

namespace P5M.Controllers
{
    public class PenggunaController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public PenggunaController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var activePenggunaList = _dbContext.Pengguna.Where(p => p.status == 1).ToList();
            return View(activePenggunaList);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
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
            return View();
        }

        [HttpPost]
        public IActionResult Create(PenggunaModel penggunaModel)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Pengguna.Add(penggunaModel);
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Data berhasil ditambahkan";
                return RedirectToAction("Index");
            }

            return View(penggunaModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            PenggunaModel penggunaModel = _dbContext.Pengguna.Find(id);
            if (penggunaModel == null)
            {
                return NotFound();
            }
            return View(penggunaModel);
        }

        [HttpPost]
        public IActionResult Edit(PenggunaModel penggunaModel)
        {
            if (ModelState.IsValid)
            {
                PenggunaModel existingPenggunaModel = _dbContext.Pengguna.Find(penggunaModel.id);

                if (existingPenggunaModel == null)
                {
                    return NotFound();
                }

                // Update the properties accordingly
                existingPenggunaModel.nama_pengguna = penggunaModel.nama_pengguna;
                existingPenggunaModel.role = penggunaModel.role;
                existingPenggunaModel.kelas = penggunaModel.kelas;
                existingPenggunaModel.status = penggunaModel.status;

                _dbContext.Pengguna.Update(existingPenggunaModel);
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Data Pengguna berhasil diupdate.";
                return RedirectToAction("Index");
            }
            return View(penggunaModel);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var response = new { success = false, message = "Gagal menghapus Data Pengguna." };
            try
            {
                PenggunaModel penggunaModel = _dbContext.Pengguna.Find(id);
                if (penggunaModel != null)
                {
                    // Instead of directly removing, update the status to 0
                    penggunaModel.status = 0;
                    _dbContext.Pengguna.Update(penggunaModel);
                    _dbContext.SaveChanges();

                    response = new { success = true, message = "Data Pengguna berhasil dihapus." };
                }
                else
                {
                    response = new { success = false, message = "Data Pengguna tidak ditemukan." };
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
