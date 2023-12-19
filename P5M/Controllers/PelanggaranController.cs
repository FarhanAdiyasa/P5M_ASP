using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P5M.Models;
using System;
using System.Linq;

namespace P5M.Controllers
{
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
                if (id != null)
                {
                    _dbContext.Pelanggaran.Remove(_dbContext.Pelanggaran.FirstOrDefault(x => x.id == id));
                    _dbContext.SaveChanges();
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
    }
}
