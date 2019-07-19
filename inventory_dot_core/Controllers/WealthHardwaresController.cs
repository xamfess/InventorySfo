﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using inventory_dot_core.Models;
using Microsoft.AspNetCore.Routing;
using inventory_dot_core.Classes;
using Microsoft.AspNetCore.Authorization;

namespace inventory_dot_core.Controllers
{
    [Authorize(Policy = "RefEditorsRole")]   
    public class WealthHardwaresController : Controller
    {
        private readonly InventoryContext _context;
        private readonly ControlesItems _ControlesItems;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public WealthHardwaresController(InventoryContext context)
        {
            _context = context;
            _ControlesItems = new ControlesItems(_context);
        }

        // GET: WealthHardwares
        public async Task<IActionResult> Index(
            string filter = "",
            string filterInv = "",
            string filterName = "",
            string filterRegion = "",
            string filterCat = "",
            string filterType = "",
            string filterOffice = "",
            int page = 1,
            string sortExpression = "WhardId")
        {
            ViewBag.Filter = filter;
            ViewBag.FilterInv = filterInv;
            ViewBag.FilterName = filterName;
            ViewBag.FilterRegion = filterRegion;
            ViewBag.FilterCat = filterCat;
            ViewBag.FilterType = filterType;
            ViewBag.FilterOffice = filterOffice;
            ViewBag.Page = page;
            ViewBag.SortExpression = sortExpression;
            //ViewBag.TableFilter = tableFilter;

            var inventoryContext = _context.WealthHardware
                .Include(w => w.WhardMolEmployee)
                .Include(w => w.WhardOffice)
                .Include(w => w.WhardRegion)
                .Include(w => w.WhardWcat)
                .Include(w => w.WhardWtype)
                .AsQueryable();

            int pageSize = 5;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToUpper();
                inventoryContext = inventoryContext.Where(e => EF.Functions.Like(e.WhardFnumber.ToUpper(), "%" + filter + "%")
                    || EF.Functions.Like(e.WhardInumber.ToUpper(), "%" + filter + "%")
                    || EF.Functions.Like(e.WhardName.ToUpper(), "%" + filter + "%")
                    || EF.Functions.Like(e.WhardOffice.OfficeName.ToUpper(), "%" + filter + "%")
                    || EF.Functions.Like(e.WhardRegion.RegionName.ToUpper(), "%" + filter + "%")
                );
            }

            if (!string.IsNullOrWhiteSpace(filterType)|| !string.IsNullOrWhiteSpace(filterInv) ||
                !string.IsNullOrWhiteSpace(filterRegion) || !string.IsNullOrWhiteSpace(filterOffice) ||
                !string.IsNullOrWhiteSpace(filterCat) || !string.IsNullOrWhiteSpace(filterName))
            {

                filterCat = !string.IsNullOrWhiteSpace(filterCat) ? filterCat.ToUpper() : null;
                filterType = !string.IsNullOrWhiteSpace(filterType) ? filterType.ToUpper() : null;
                filterRegion = !string.IsNullOrWhiteSpace(filterRegion) ? filterRegion.ToUpper() : null;
                filterName = !string.IsNullOrWhiteSpace(filterName) ? filterName.ToUpper() : null;
                filterOffice = !string.IsNullOrWhiteSpace(filterOffice) ? filterOffice.ToUpper() : null;
                filterInv = !string.IsNullOrWhiteSpace(filterInv) ? filterInv.ToUpper() : null;

                //filter = filter.ToUpper();
                inventoryContext = inventoryContext.Where(e => EF.Functions.Like(e.WhardWtype.WtypeName.ToUpper(), "%" + filterType + "%")
                    || EF.Functions.Like(e.WhardInumber.ToUpper(), "%" + filterInv + "%")
                    || EF.Functions.Like(e.WhardName.ToUpper(), "%" + filterName + "%")
                    || EF.Functions.Like(e.WhardOffice.OfficeName.ToUpper(), "%" + filterOffice + "%")
                    || EF.Functions.Like(e.WhardRegion.RegionName.ToUpper(), "%" + filterRegion + "%")
                    || EF.Functions.Like(e.WhardWcat.Wcatname.ToUpper(), "%" + filterCat + "%")
                );
            }
            var model = await Classes.Paging.PagingList.CreateAsync
                (
                   inventoryContext, pageSize, page, sortExpression, "WhardId"
                   );

            model.RouteValue = new RouteValueDictionary {
                { "filter", filter},
                { "FilterInv", filterInv},
                { "filterOffice", filterOffice},
                { "filterName", filterName},
                { "filterCat", filterCat},
                { "filterType", filterType},
                { "filterRegion", filterRegion}
            };

            ViewData["WhardRegionFilter"] = new SelectList(_context.Region, "RegionId", "RegionName");

            return View(model);
        }

        // GET: WealthHardwares/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wealthHardware = await _context.WealthHardware
                .Include(w => w.WhardMolEmployee)
                .Include(w => w.WhardOffice)
                .Include(w => w.WhardRegion)
                .Include(w => w.WhardWcat)
                .Include(w => w.WhardWtype)
                .FirstOrDefaultAsync(m => m.WhardId == id);
            if (wealthHardware == null)
            {
                return NotFound();
            }

            return View(wealthHardware);
        }

        // GET: WealthHardwares/Create
        public IActionResult Create(string filter = "", int page = 1, string sortExpression = "WhardId")
        {
            ViewBag.Filter = filter;
            ViewBag.Page = page;
            ViewBag.SortExpression = sortExpression;

            var _Region = _context.Region.FirstOrDefault();

            ViewData["WhardMolEmployeeId"] = _ControlesItems.GetMOLEmployeesByRegion(_Region.RegionId);
            ViewData["WhardOfficeId"] = _ControlesItems.GetOfficesByRegion(_Region.RegionId);
            ViewData["WhardRegionId"] = new SelectList(_context.Region, "RegionId", "RegionName");
            ViewData["WhardWcatId"] = new SelectList(_context.WealthCategories, "WcatId", "Wcatname");
            ViewData["WhardWtypeId"] = new SelectList(_context.WealthTypes, "WtypeId", "WtypeName");
            return View();
        }

        // POST: WealthHardwares/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WhardId,WhardInumber,WhardFnumber,WhardWcatId,WhardWtypeId,WhardName" +
            ",WhardDateOfAdoption,WhardInitialCost,WhardResidualValue,WhardOfficeId" +
            ",WhardNote,WhardArchiv,WhardCreateDate,WhardMolEmployeeId,WhardRegionId,IsSoftDeployed")] WealthHardware wealthHardware,
            bool WhardArchiv, bool IsSoftDeployed,
            string filter = "", int page = 1, string sortExpression = "WhardId")
        {
            ViewBag.Filter = filter;
            ViewBag.Page = page;
            ViewBag.SortExpression = sortExpression;

            wealthHardware.WhardArchiv = WhardArchiv ? 1 : 0;
            wealthHardware.IsSoftDeployed = IsSoftDeployed ? 1 : 0;

            ModelState.Clear();

            if (ModelState.IsValid)
            {
                _context.Add(wealthHardware);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index),
                    new
                    {
                        filter = filter,
                        page = page,
                        sortExpression = sortExpression
                    });
            }
            ViewData["WhardMolEmployeeId"] = _ControlesItems.GetMOLEmployeesByRegion(wealthHardware.WhardRegion.RegionId);
            ViewData["WhardOfficeId"] = _ControlesItems.GetOfficesByRegion(wealthHardware.WhardRegion.RegionId);
            ViewData["WhardRegionId"] = new SelectList(_context.Region, "RegionId", "RegionName", wealthHardware.WhardRegionId);
            ViewData["WhardWcatId"] = new SelectList(_context.WealthCategories, "WcatId", "Wcatname", wealthHardware.WhardWcatId);
            ViewData["WhardWtypeId"] = new SelectList(_context.WealthTypes, "WtypeId", "WtypeName", wealthHardware.WhardWtypeId);
            return View(wealthHardware);
        }

        // GET: WealthHardwares/Edit/5
        public async Task<IActionResult> Edit(int? id, string filter = "", int page = 1, string sortExpression = "WhardId")
        {
            ViewBag.Filter = filter;
            ViewBag.Page = page;
            ViewBag.SortExpression = sortExpression;

            if (id == null)
            {
                return NotFound();
            }

            var wealthHardware = await _context.WealthHardware.FindAsync(id);
            if (wealthHardware == null)
            {
                return NotFound();
            }

            var _whard = _context.WealthHardware.Include(r => r.WhardRegion).AsNoTracking();

            _whard = _whard.Where(h => h.WhardId == id);

            ViewData["WhardMolEmployeeId"] = _ControlesItems.GetMOLEmployeesByRegion(_whard.First().WhardRegion.RegionId);
            ViewData["WhardOfficeId"] = _ControlesItems.GetOfficesByRegion(_whard.First().WhardRegion.RegionId);
            ViewData["WhardRegionId"] = new SelectList(_context.Region, "RegionId", "RegionName", wealthHardware.WhardRegionId);
            ViewData["WhardWcatId"] = new SelectList(_context.WealthCategories, "WcatId", "Wcatname", wealthHardware.WhardWcatId);
            ViewData["WhardWtypeId"] = new SelectList(_context.WealthTypes, "WtypeId", "WtypeName", wealthHardware.WhardWtypeId);
            return View(wealthHardware);
        }

        // POST: WealthHardwares/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WhardId,WhardInumber,WhardFnumber,WhardWcatId,WhardWtypeId" +
            ",WhardName,WhardDateOfAdoption,WhardInitialCost,WhardResidualValue,WhardOfficeId,WhardNote" +
            ",WhardArchiv,WhardCreateDate,WhardMolEmployeeId,WhardRegionId,IsSoftDeployed")] WealthHardware wealthHardware,
            bool WhardArchiv, bool IsSoftDeployed,
            string filter = "", int page = 1, string sortExpression = "WhardId")
        {
            ViewBag.Filter = filter;
            ViewBag.Page = page;
            ViewBag.SortExpression = sortExpression;

            wealthHardware.WhardArchiv = WhardArchiv ? 1 : 0;
            wealthHardware.IsSoftDeployed = IsSoftDeployed ? 1 : 0;

            ModelState.Clear();

            if (id != wealthHardware.WhardId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wealthHardware);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WealthHardwareExists(wealthHardware.WhardId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index),
                     new
                     {
                         filter = filter,
                         page = page,
                         sortExpression = sortExpression
                     });
            }

            var _whard = _context.WealthHardware.Include(r => r.WhardRegion).AsNoTracking();

            _whard = _whard.Where(h => h.WhardId == id);

            ViewData["WhardMolEmployeeId"] = _ControlesItems.GetMOLEmployeesByRegion(_whard.First().WhardRegion.RegionId);
            ViewData["WhardOfficeId"] = _ControlesItems.GetOfficesByRegion(_whard.First().WhardRegion.RegionId);
            ViewData["WhardRegionId"] = new SelectList(_context.Region, "RegionId", "RegionName", wealthHardware.WhardRegionId);
            ViewData["WhardWcatId"] = new SelectList(_context.WealthCategories, "WcatId", "Wcatname", wealthHardware.WhardWcatId);
            ViewData["WhardWtypeId"] = new SelectList(_context.WealthTypes, "WtypeId", "WtypeName", wealthHardware.WhardWtypeId);
            return View(wealthHardware);
        }

        // GET: WealthHardwares/Delete/5
        public async Task<IActionResult> Delete(int? id, string filter = "", int page = 1, string sortExpression = "WhardId")
        {
            ViewBag.Filter = filter;
            ViewBag.Page = page;
            ViewBag.SortExpression = sortExpression;

            if (id == null)
            {
                return NotFound();
            }

            var wealthHardware = await _context.WealthHardware
                .Include(w => w.WhardMolEmployee)
                .Include(w => w.WhardOffice)
                .Include(w => w.WhardRegion)
                .Include(w => w.WhardWcat)
                .Include(w => w.WhardWtype)
                .FirstOrDefaultAsync(m => m.WhardId == id);
            if (wealthHardware == null)
            {
                return NotFound();
            }

            return View(wealthHardware);
        }

        // POST: WealthHardwares/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string filter = "", int page = 1, string sortExpression = "WhardId")
        {
            ViewBag.Filter = filter;
            ViewBag.Page = page;
            ViewBag.SortExpression = sortExpression;

            var wealthHardware = await _context.WealthHardware.FindAsync(id);
            _context.WealthHardware.Remove(wealthHardware);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index),
                 new
                 {
                     filter = filter,
                     page = page,
                     sortExpression = sortExpression
                 });
        }

        private bool WealthHardwareExists(int id)
        {
            return _context.WealthHardware.Any(e => e.WhardId == id);
        }
    }
}
