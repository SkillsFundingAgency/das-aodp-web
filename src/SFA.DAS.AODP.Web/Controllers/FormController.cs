using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Infrastructure.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class FormController : Controller
{
    private readonly IGenericRepository<Form> _formRepository;
    private readonly IGenericRepository<Section> _sectionRepository;

    public FormController(IGenericRepository<Form> formRepository, IGenericRepository<Section> sectionRepository) {
        _formRepository = formRepository;
        _sectionRepository = sectionRepository;
    }

    #region Main
    public IActionResult Index()
    {
        var forms = _formRepository.GetAll().ToList();
        if (!forms.Any()) return RedirectToAction(nameof(Create));
        return View(forms);
    }
    #endregion

    #region Create
    public IActionResult Create()
    {
        return View(new Form());
    }

    [HttpPost]
    public IActionResult Create(Form form)
    {
        _formRepository.Add(form);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Edit
    public IActionResult Edit(Guid id)
    {
        var form = _formRepository.GetById(id);
        if (form == null) return NotFound();

        ViewData["Sections"] = _sectionRepository.GetAll().Where(s => s.FormId == form.Id).ToList();

        return View(form);
    }

    [HttpPost]
    public IActionResult Edit(Form form)
    {
        _formRepository.Update(form);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Delete
    public IActionResult Delete(Guid id)
    {
        var form = _formRepository.GetById(id);
        if (form == null) return NotFound();
        return View(form);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(Guid id)
    {
        _formRepository.Delete(id);
        return RedirectToAction(nameof(Index));
    }
    #endregion
}