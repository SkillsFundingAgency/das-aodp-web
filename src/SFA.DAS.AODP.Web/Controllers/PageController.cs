using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Infrastructure.MemoryCache;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class PageController : BaseController
{
    public PageController(ICacheManager cacheManager) : base(cacheManager) { }

    #region Create
    public IActionResult Create(Guid sectionId)
    {
        var page = new Page { SectionId = sectionId };
        ViewData["Pages"] = GetFromCache<List<Page>>("Pages")?.Where(s => s.SectionId == page.SectionId).ToList() ?? new List<Page>();
        return View(page);
    }

    [HttpPost]
    public IActionResult Create(Page page)
    {
        var pages = GetFromCache<List<Page>>("Pages") ?? new List<Page>();
        page.Id = Guid.NewGuid();//pages.Count + 1;
        pages.Add(page);
        SetToCache("Pages", pages);
        return RedirectToAction(nameof(Edit), new { sectionId = page.SectionId });
    }
    #endregion

    #region Edit
    public IActionResult Edit(Guid id)
    {
        var page = GetFromCache<List<Page>>("Pages")?.FirstOrDefault(p => p.Id == id);
        ViewData["Pages"] = GetFromCache<List<Page>>("Pages")?.Where(s => s.SectionId == page.SectionId).ToList() ?? new List<Page>();
        if (page == null) return NotFound();
        return View(page);
    }

    [HttpPost]
    public IActionResult Edit(Page page)
    {
        var pages = GetFromCache<List<Page>>("Pages") ?? new List<Page>();
        var index = pages.FindIndex(p => p.Id == page.Id);
        if (index == -1) return NotFound();

        pages[index] = page;
        SetToCache("Pages", pages);
        return RedirectToAction(nameof(Index), new { sectionId = page.SectionId });
    }
    #endregion

    #region Delete
    public IActionResult Delete(Guid id)
    {
        var page = GetFromCache<List<Page>>("Pages")?.FirstOrDefault(p => p.Id == id);
        if (page == null) return NotFound();
        return View(page);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(Guid id)
    {
        var pages = GetFromCache<List<Page>>("Pages") ?? new List<Page>();
        var page = pages.FirstOrDefault(p => p.Id == id);
        if (page != null)
        {
            pages.Remove(page);
            SetToCache("Pages", pages);
            return RedirectToAction(nameof(Edit), "Section", new { id = page.SectionId });
        }
        return NotFound();
    }
    #endregion
}