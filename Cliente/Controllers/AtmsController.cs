using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cliente.Models;

namespace Cliente.Controllers
{
    public class AtmsController : Controller
    {
        private readonly clientedbcontext _context;

        public AtmsController(clientedbcontext context)
        {
            _context = context;
        }

        // GET: Atms
        public async Task<IActionResult> Index()
        {
            var clientedbcontext = _context.atms.Include(a => a.ClienteModel);
            return View(await clientedbcontext.ToListAsync());
        }

        // GET: Atms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atmsModel = await _context.atms
                .Include(a => a.ClienteModel)
                .FirstOrDefaultAsync(m => m.id == id);
            if (atmsModel == null)
            {
                return NotFound();
            }

            return View(atmsModel);
        }

        // GET: Atms/Create
        public IActionResult Create()
        {
            ViewData["ClienteModelid"] = new SelectList(_context.Clientes, "id", "Apellido");
            return View();
        }

        // POST: Atms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,NombreAgencia,ClienteModelid")] AtmsModel atmsModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(atmsModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteModelid"] = new SelectList(_context.Clientes, "id", "Apellido", atmsModel.ClienteModelid);
            return View(atmsModel);
        }

        // GET: Atms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atmsModel = await _context.atms.FindAsync(id);
            if (atmsModel == null)
            {
                return NotFound();
            }
            ViewData["ClienteModelid"] = new SelectList(_context.Clientes, "id", "Apellido", atmsModel.ClienteModelid);
            return View(atmsModel);
        }

        // POST: Atms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,NombreAgencia,ClienteModelid")] AtmsModel atmsModel)
        {
            if (id != atmsModel.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(atmsModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AtmsModelExists(atmsModel.id))
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
            ViewData["ClienteModelid"] = new SelectList(_context.Clientes, "id", "Apellido", atmsModel.ClienteModelid);
            return View(atmsModel);
        }

        // GET: Atms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var atmsModel = await _context.atms
                .Include(a => a.ClienteModel)
                .FirstOrDefaultAsync(m => m.id == id);
            if (atmsModel == null)
            {
                return NotFound();
            }

            return View(atmsModel);
        }

        // POST: Atms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var atmsModel = await _context.atms.FindAsync(id);
            if (atmsModel != null)
            {
                _context.atms.Remove(atmsModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AtmsModelExists(int id)
        {
            return _context.atms.Any(e => e.id == id);
        }
    }
}
