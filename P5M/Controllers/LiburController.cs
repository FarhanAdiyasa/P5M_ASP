using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P5M.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace P5M.Controllers
{
    [Authorize]
    public class LiburController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public LiburController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var activeLiburList = _dbContext.Libur.Where(m => m.status == 1).ToList();
            return View(activeLiburList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(LiburModel liburModel)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Libur.Add(liburModel);
                _dbContext.SaveChanges();
                AddLog("Tambah Libur " + liburModel.deskripsi, DateTime.Now);
                TempData["SuccessMessage"] = "Data berhasil ditambahkan";
                return RedirectToAction("Index");
            }

            return View(liburModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            LiburModel liburModel = _dbContext.Libur.Find(id);
            if (liburModel == null)
            {
                return NotFound();
            }
            return View(liburModel);
        }

        [HttpPost]
        public IActionResult Edit(LiburModel liburModel)
        {
            if (ModelState.IsValid)
            {
                LiburModel newLiburModel = _dbContext.Libur.Find(liburModel.id);

                if (newLiburModel == null)
                {
                    return NotFound();
                }

                // Simpan nilai lama untuk digunakan dalam log
                string deskripsiLama = newLiburModel.deskripsi;
                DateTime tanggalLama = newLiburModel.tanggal;

                newLiburModel.tanggal = liburModel.tanggal;
                newLiburModel.deskripsi = liburModel.deskripsi;
                newLiburModel.kategori = liburModel.kategori;

                _dbContext.Libur.Update(newLiburModel);
                _dbContext.SaveChanges();

                // Tambahkan log dengan informasi deskripsi dan tanggal lama
                AddLog("Update Libur" + newLiburModel.deskripsi, DateTime.Now);

                TempData["SuccessMessage"] = "Data Libur berhasil diupdate.";
                return RedirectToAction("Index");
            }
            return View(liburModel);
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            var response = new { success = false, message = "Gagal menghapus Data Libur." };
            try
            {
                LiburModel liburModel = _dbContext.Libur.Find(id);
                if (liburModel != null)
                {
                    liburModel.status = 0;
                    _dbContext.Libur.Update(liburModel);
                    _dbContext.SaveChanges();
                    AddLog("Hapus Libur " + liburModel.deskripsi, DateTime.Now);
                    response = new { success = true, message = "Data Libur berhasil dihapus." };
                }
                else
                {
                    response = new { success = false, message = "Data Libur tidak ditemukan." };
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
