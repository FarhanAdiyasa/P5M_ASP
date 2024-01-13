using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P5M.Models;
using System;
using System.Linq;

namespace P5M.Controllers
{
    [Authorize]
    public class PelanggaranController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public PelanggaranController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var activePelanggaranList = _dbContext.Pelanggaran.Where(m => m.status == 1).ToList();
            return View(activePelanggaranList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(PelanggaranModel pelanggaranModel)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Pelanggaran.Add(pelanggaranModel);
                _dbContext.SaveChanges();
                AddLog("Tambah Pelanggaran " + pelanggaranModel.nama_pelanggaran, DateTime.Now);
                TempData["SuccessMessage"] = "Data berhasil ditambahkan";
                return RedirectToAction("Index");
            }

            return View(pelanggaranModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            PelanggaranModel pelanggaranModel = _dbContext.Pelanggaran.Find(id);
            if (pelanggaranModel == null)
            {
                return NotFound();
            }
            return View(pelanggaranModel);
        }

        [HttpPost]
        public IActionResult Edit(PelanggaranModel pelanggaranModel)
        {
            if (ModelState.IsValid)
            {
                PelanggaranModel newPelanggaranModel = _dbContext.Pelanggaran.Find(pelanggaranModel.id);

                if (newPelanggaranModel == null)
                {
                    return NotFound();
                }

                newPelanggaranModel.nama_pelanggaran = pelanggaranModel.nama_pelanggaran;
                newPelanggaranModel.jam_minus = pelanggaranModel.jam_minus;
                newPelanggaranModel.status = pelanggaranModel.status;

                _dbContext.Pelanggaran.Update(newPelanggaranModel);
                _dbContext.SaveChanges();
                AddLog("Update Pelanggaran " + pelanggaranModel.nama_pelanggaran, DateTime.Now);
                TempData["SuccessMessage"] = "Data Pelanggaran berhasil diupdate.";
                return RedirectToAction("Index");
            }
            return View(pelanggaranModel);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var response = new { success = false, message = "Gagal menghapus Data Pelanggaran." };
            try
            {
                PelanggaranModel pelanggaranModel = _dbContext.Pelanggaran.Find(id);
                if (pelanggaranModel != null)
                {
                    // Instead of directly removing, update the status to 0
                    pelanggaranModel.status = 0;
                    _dbContext.Pelanggaran.Update(pelanggaranModel);
                    _dbContext.SaveChanges();
                    AddLog("Hapus Pelanggaran " + pelanggaranModel.nama_pelanggaran, DateTime.Now);
                    response = new { success = true, message = "Data Pelanggaran berhasil dihapus." };
                }
                else
                {
                    response = new { success = false, message = "Data Pelanggaran tidak ditemukan." };
                }
            }
            catch (Exception ex)
            {
                response = new { success = false, message = ex.Message };
            }
            return Json(response);
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
