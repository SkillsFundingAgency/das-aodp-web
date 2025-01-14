using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Infrastructure.MemoryCache;
using SFA.DAS.AODP.Infrastructure.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class SectionController : Controller
{
    private readonly IGenericRepository<Section> _sectionRepository;
    private readonly IGenericRepository<Page> _pageRepository;

    public SectionController(IGenericRepository<Section> sectionRepository, IGenericRepository<Page> pageRepository)
    {
        _sectionRepository = sectionRepository;
        _pageRepository = pageRepository;
    }

    #region Create
    public IActionResult Create(Guid formId)
    {
        ViewData["Sections"] = _sectionRepository.GetAll().Where(p => p.FormId == formId).ToList();
        return View(new Section { FormId = formId });
    }

    [HttpPost]
    public IActionResult Create(Section section)
    {
        _sectionRepository.Add(section);
        return RedirectToAction("Edit", "Form", new { id = section.FormId });
    }
    #endregion

    #region Edit
    public IActionResult Edit(Guid id)
    {
        var section = _sectionRepository.GetById(id);
        if (section == null) return NotFound();

        ViewData["Sections"] = _sectionRepository.GetAll().Where(p => p.FormId == section.FormId).ToList();
        ViewData["Pages"] = _pageRepository.GetAll().Where(p => p.SectionId == section.Id).ToList();

        return View(section);
    }

    [HttpPost]
    public IActionResult Edit(Section section)
    {
        _sectionRepository.Update(section);
        return RedirectToAction("Edit", "Form", new { id = section.FormId });
    }
    #endregion

    #region Delete
    public IActionResult Delete(Guid id)
    {
        var section = _sectionRepository.GetById(id);
        if (section == null) return NotFound();
        return View(section);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(Guid id)
    {
        var section = _sectionRepository.GetById(id);
        if (section != null)
        {
            _sectionRepository.Delete(id);
            return RedirectToAction("Edit", "Form", new { id = section.FormId });
        }
        return NotFound();
    }
    #endregion
}