using ClosedXML.Excel;
using EducateApp.Models;
using EducateApp.Models.Data;
using EducateApp.ViewModels.Disciplines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EducateApp.Controllers
{
    [Authorize(Roles = "admin, registeredUser")]
    public class DisciplinesController : Controller
    {
        private readonly AppCtx _context;
        private readonly UserManager<User> _userManager;

        public DisciplinesController(AppCtx context, UserManager<User> user)
        {
            _context = context;
            _userManager = user;
        }

        // GET: Disciplines
        public async Task<IActionResult> Index()
        {
            IdentityUser user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            var appCtx = _context.Disciplines
                .Include(s => s.User)
                .Where(s => s.IdUser == user.Id)
                .OrderBy(f => f.IndexProfMod)
                .ThenBy(f => f.Index);

            return View(await appCtx.ToListAsync());
        }

        // GET: Disciplines/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Disciplines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDisciplineViewModel model)
        {
            IdentityUser user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            if (_context.Disciplines
                .Where(f => f.IdUser == user.Id &&
                    f.IndexProfMod == model.IndexProfMod &&
                    f.NameProfMod == model.NameProfMod &&
                    f.Index == model.Index &&
                    f.Name == model.Name &&
                    f.ShortName == model.ShortName)
                .FirstOrDefault() != null)
            {
                ModelState.AddModelError("", "Введеная дисциплина уже существует");
            }

            if (ModelState.IsValid)
            {
                Discipline discipline = new()
                {
                    IndexProfMod = model.IndexProfMod,
                    NameProfMod = model.NameProfMod,
                    Index = model.Index,
                    Name = model.Name,
                    ShortName = model.ShortName,
                    IdUser = user.Id
                };

                _context.Add(discipline);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Disciplines/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discipline = await _context.Disciplines.FindAsync(id);
            if (discipline == null)
            {
                return NotFound();
            }

            EditDisciplineViewModel model = new()
            {
                Id = discipline.Id,
                IndexProfMod = discipline.IndexProfMod,
                NameProfMod = discipline.NameProfMod,
                Index = discipline.Index,
                Name = discipline.Name,
                ShortName = discipline.ShortName,
                IdUser = discipline.IdUser
            };

            return View(model);
        }

        // POST: Disciplines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, EditDisciplineViewModel model)
        {
            Discipline discipline = await _context.Disciplines.FindAsync(id);
            IdentityUser user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            if (_context.Disciplines
                .Where(f => f.IdUser == user.Id &&
                    f.IndexProfMod == model.IndexProfMod &&
                    f.NameProfMod == model.NameProfMod &&
                    f.Index == model.Index &&
                    f.Name == model.Name &&
                    f.ShortName == model.ShortName)
                .FirstOrDefault() != null)
            {
                ModelState.AddModelError("", "Введеная дисциплина уже существует");
            }

            if (id != discipline.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    discipline.IndexProfMod = model.IndexProfMod;
                    discipline.NameProfMod = model.NameProfMod;
                    discipline.Index = model.Index;
                    discipline.Name = model.Name;
                    discipline.ShortName = model.ShortName;
                    _context.Update(discipline);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DisciplineExists(discipline.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Disciplines/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discipline = await _context.Disciplines
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (discipline == null)
            {
                return NotFound();
            }

            return View(discipline);
        }

        // POST: Disciplines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var discipline = await _context.Disciplines.FindAsync(id);
            _context.Disciplines.Remove(discipline);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DisciplineExists(short id)
        {
            return _context.Disciplines.Any(e => e.Id == id);
        }

        // GET: FormsOfStudy/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Discipline = await _context.Disciplines
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Discipline == null)
            {
                return NotFound();
            }

            return View(Discipline);
        }

        public async Task<FileResult> DownloadPattern()
        {
            IdentityUser user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            // выбираем из базы данных все специальности текущего пользователя
            var appCtx = _context.Disciplines
                .Include(s => s.User)
                .Where(s => s.IdUser == user.Id)
                .OrderBy(f => f.IndexProfMod)
                .ThenBy(f => f.Index);

            int i = 1;      // счетчик

            IXLRange rngBorder;     // объект для работы с диапазонами в Excel (выделение групп ячеек)

            // создание книги Excel
            using (XLWorkbook workbook = new(XLEventTracking.Disabled))
            {
                // добавить лист в книгу Excel
                IXLWorksheet worksheet = workbook.Worksheets
                    .Add("Дисциплины");

                // в первой строке текущего листа указываем: 
                // в ячейку A1 значение "Дисциплина"
                worksheet.Cell("A" + i).Value = "Дисциплины";
                // увеличение счетчика на единицу
                i+=2;

                // во второй строке
                worksheet.Cell("A" + i).Value = "Индекс профессионального модуля";
                worksheet.Cell("B" + i).Value = "Профессиональный модуль";
                worksheet.Cell("C" + i).Value = "Индекс дисциплины";
                worksheet.Cell("D" + i).Value = "Название дисциплины";
                worksheet.Cell("E" + i).Value = "Сокращённое название";

                // устанавливаем внешние границы для диапазона A4:F4
                rngBorder = worksheet.Range("A3:E3");       // создание диапазона (выделения ячеек)
                rngBorder.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;       // для диапазона задаем внешнюю границу

                // на листе для столбцов задаем значение ширины по содержимому
                worksheet.Columns().AdjustToContents();

                // создаем стрим
                using (MemoryStream stream = new())
                {
                    // помещаем в стрим созданную книгу
                    workbook.SaveAs(stream);
                    stream.Flush();

                    // возвращаем файл определенного типа
                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"disciplines_{DateTime.UtcNow.ToShortDateString()}.xlsx"     //в названии файла указываем таблицу и текущую дату
                    };
                }
            }
        }

        private bool DisciplinesExists(short id)
        {
            return _context.Disciplines.Any(e => e.Id == id);
        }
    }
}
