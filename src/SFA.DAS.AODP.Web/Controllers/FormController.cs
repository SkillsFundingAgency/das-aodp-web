using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Infrastructure.MemoryCache;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class FormController : BaseController
{
    public FormController(ICacheManager cacheManager) : base(cacheManager) { }

    #region Main
    public IActionResult Index()
    {
        var forms = GetFromCache<List<Form>>("Forms") ?? new List<Form>();
        if (forms.Count() == 0) return RedirectToAction(nameof(Create));
        return View(forms);
    }
    #endregion

    #region Create
    public IActionResult Create()
    {
        var form = new Form();
        return View(form);
    }

    [HttpPost]
    public IActionResult Create(Form form)
    {
        var forms = GetFromCache<List<Form>>("Forms") ?? new List<Form>();
        form.Id = Guid.NewGuid();//forms.Count + 1;
        forms.Add(form);
        SetToCache("Forms", forms);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Edit
    public IActionResult Edit(Guid id)
    {
        var form = GetFromCache<List<Form>>("Forms")?.FirstOrDefault(f => f.Id == id);
        ViewData["Sections"] = GetFromCache<List<Section>>("Sections")?.Where(s => s.FormId == id).ToList() ?? new List<Section>();
        if (form == null) return NotFound();
        return View(form);
    }

    [HttpPost]
    public IActionResult Edit(Form form)
    {
        var forms = GetFromCache<List<Form>>("Forms") ?? new List<Form>();
        var index = forms.FindIndex(f => f.Id == form.Id);
        if (index == -1) return NotFound();

        forms[index] = form;
        SetToCache("Forms", forms);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Delete
    public IActionResult Delete(Guid id)
    {
        var form = GetFromCache<List<Form>>("Forms")?.FirstOrDefault(f => f.Id == id);
        if (form == null) return NotFound();
        return View(form);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(Guid id)
    {
        var forms = GetFromCache<List<Form>>("Forms") ?? new List<Form>();
        forms.RemoveAll(f => f.Id == id);
        SetToCache("Forms", forms);

        if (forms.Count() == 0) return RedirectToAction(nameof(Create));

        return RedirectToAction("Index", "Form", null);
    }
    #endregion
}