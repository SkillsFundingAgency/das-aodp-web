using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Infrastructure.MemoryCache;
using SFA.DAS.AODP.Infrastructure.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class PageController : Controller
{
    private readonly IGenericRepository<Page> _pageRepository;

    public PageController(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    #region Create
    public IActionResult Create(Guid sectionId)
    {
        ViewData["Pages"] = _pageRepository.GetAll().Where(p => p.SectionId == sectionId).ToList();
        return View(new Page { SectionId = sectionId });
    }

    [HttpPost]
    public IActionResult Create(Page page)
    {
        //page.Id = Guid.NewGuid();
        _pageRepository.Add(page);
        return RedirectToAction("Edit", "Section", new { id = page.SectionId });
    }
    #endregion

    #region Edit
    public IActionResult Edit(Guid id)
    {
        var page = _pageRepository.GetById(id);
        if (page == null) return NotFound();

        ViewData["Pages"] = _pageRepository.GetAll().Where(p => p.SectionId == page.SectionId).ToList();

        return View(page);
    }

    [HttpPost]
    public IActionResult Edit(Page page)
    {
        _pageRepository.Update(page);
        return RedirectToAction("Edit", "Section", new { id = page.SectionId });
    }
    #endregion

    #region Delete
    public IActionResult Delete(Guid id)
    {
        var page = _pageRepository.GetById(id);
        if (page == null) return NotFound();
        return View(page);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(Guid id)
    {
        var page = _pageRepository.GetById(id);
        if (page != null)
        {
            _pageRepository.Delete(id);
            return RedirectToAction("Edit", "Section", new { id = page.SectionId });
        }
        return NotFound();
    }
    #endregion
}
