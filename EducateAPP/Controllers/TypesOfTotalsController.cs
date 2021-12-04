using EducateApp.Models;
using EducateApp.Models.Data;
using EducateApp.ViewModels.TypesOfTotals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EducateApp.Controllers
{
    [Authorize(Roles = "admin, registeredUser")]
    public class TypesOfTotalsController : Controller
    {
        private readonly AppCtx _context;
        private readonly UserManager<User> _userManager;

        public TypesOfTotalsController(AppCtx context, UserManager<User> user)
        {
            _context = context;
            _userManager = user;
        }

        // GET: TypesOfTotals
        public async Task<IActionResult> Index(TypeOfTotalSortState sortOrder = TypeOfTotalSortState.CertificateNameAsc)
        {
            IdentityUser user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            var typesOfTotals = _context.TypesOfTotals
                .Include(s => s.User)
                .Where(s => s.IdUser == user.Id);

            ViewData["CertificateNameSort"] = sortOrder == TypeOfTotalSortState.CertificateNameAsc ? TypeOfTotalSortState.CertificateNameDesc : TypeOfTotalSortState.CertificateNameAsc;

            typesOfTotals = sortOrder switch
            {
                TypeOfTotalSortState.CertificateNameDesc => typesOfTotals.OrderByDescending(s => s.CertificateName),
                _ => typesOfTotals.OrderBy(s => s.CertificateName),
            };

            // возвращаем в представление полученный список записей
            return View(await typesOfTotals.AsNoTracking().ToListAsync());
        }

        // GET: TypesOfTotals/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TypesOfTotals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTypeOfTotalViewModel model)
        {
            IdentityUser user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            if (_context.TypesOfTotals
                .Where(f => f.IdUser == user.Id &&
                    f.CertificateName == model.CertificateName)
                .FirstOrDefault() != null)
            {
                ModelState.AddModelError("", "Введеный вид промежуточной аттестации уже существует");
            }

            if (ModelState.IsValid)
            {
                TypeOfTotal typeOfTotals = new()
                {
                    CertificateName = model.CertificateName,
                    IdUser = user.Id
                };

                _context.Add(typeOfTotals);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: TypesOfTotals/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeOfTotals = await _context.TypesOfTotals.FindAsync(id);
            if (typeOfTotals == null)
            {
                return NotFound();
            }

            EditTypeOfTotalViewModel model = new()
            {
                Id = typeOfTotals.Id,
                CertificateName = typeOfTotals.CertificateName,
                IdUser = typeOfTotals.IdUser
            };

            return View(model);
        }

        // POST: TypesOfTotals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, EditTypeOfTotalViewModel model)
        {
            TypeOfTotal typeOfTotals = await _context.TypesOfTotals.FindAsync(id);
            IdentityUser user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            if (_context.TypesOfTotals
                .Where(f => f.IdUser == user.Id &&
                    f.CertificateName == model.CertificateName)
                .FirstOrDefault() != null)
            {
                ModelState.AddModelError("", "Введеный вид промежуточной аттестации уже существует");
            }

            if (id != typeOfTotals.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    typeOfTotals.Id = model.Id;
                    typeOfTotals.CertificateName = model.CertificateName;
                    typeOfTotals.IdUser = model.IdUser;
                    _context.Update(typeOfTotals);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TypeOfTotalsExists(typeOfTotals.Id))
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

        // GET: TypesOfTotals/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeOfTotals = await _context.TypesOfTotals
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (typeOfTotals == null)
            {
                return NotFound();
            }

            return View(typeOfTotals);
        }

        // POST: TypesOfTotals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var typeOfTotals = await _context.TypesOfTotals.FindAsync(id);
            _context.TypesOfTotals.Remove(typeOfTotals);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TypeOfTotalsExists(short id)
        {
            return _context.TypesOfTotals.Any(e => e.Id == id);
        }

        // GET: TypeOfTotal/Details/4
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var TypeOfTotal = await _context.TypesOfTotals
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (TypeOfTotal == null)
            {
                return NotFound();
            }

            return PartialView(TypeOfTotal);
        }

        private bool TypeOfTotalExists(short id)
        {
            return _context.TypesOfTotals.Any(e => e.Id == id);
        }
    }
}
