using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Infrastructure.MemoryCache;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class SectionController : BaseController
{
    //Example httpClient
    private readonly HttpClient _httpClient;

    public SectionController(ICacheManager cacheManager, HttpClient httpClient) : base(cacheManager) {
        _httpClient = httpClient;
    }

    #region Create
    public IActionResult Create(Guid formId)
    {
        var section = new Section { FormId = formId };
        ViewData["Sections"] = GetFromCache<List<Section>>("Sections")?.Where(s => s.FormId == section.FormId).ToList() ?? new List<Section>();
        return View(section);
    }

    [HttpPost]
    public IActionResult Create(Section section)
    {
        var sections = GetFromCache<List<Section>>("Sections") ?? new List<Section>();
        section.Id = Guid.NewGuid();//sections.Count + 1;
        sections.Add(section);
        SetToCache("Sections", sections);
        return RedirectToAction(nameof(Edit), "Form", new { id = section.FormId });
    }
    #endregion

    #region Edit
    public async Task<ActionResult> Edit(Guid id)
    {
        var section = GetFromCache<List<Section>>("Sections")?.FirstOrDefault(s => s.Id == id);
        ViewData["Sections"] = GetFromCache<List<Section>>("Sections")?.Where(s => s.FormId == section.FormId).ToList() ?? new List<Section>();
        ViewData["Pages"] = GetFromCache<List<Page>>("Pages")?.Where(s => s.SectionId == id).ToList() ?? new List<Page>();

        if (section == null) return NotFound();
        return View(section);
    }

    [HttpPost]
    public IActionResult Edit(Section section)
    {
        var sections = GetFromCache<List<Section>>("Sections") ?? new List<Section>();
        var index = sections.FindIndex(s => s.Id == section.Id);
        if (index == -1) return NotFound();

        sections[index] = section;
        SetToCache("Sections", sections);
        return RedirectToAction(nameof(Edit), "Form", new { id = section.FormId });
    }
    #endregion

    #region Delete
    public IActionResult Delete(Guid id)
    {
        var section = GetFromCache<List<Section>>("Sections")?.FirstOrDefault(s => s.Id == id);
        if (section == null) return NotFound();
        return View(section);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(Guid id)
    {
        var sections = GetFromCache<List<Section>>("Sections") ?? new List<Section>();
        var section = sections.FirstOrDefault(s => s.Id == id);
        if (section != null)
        {
            sections.Remove(section);
            SetToCache("Sections", sections);
            return RedirectToAction(nameof(Edit), "Form", new { id = section.FormId });
        }
        return NotFound();
    }
    #endregion
}
