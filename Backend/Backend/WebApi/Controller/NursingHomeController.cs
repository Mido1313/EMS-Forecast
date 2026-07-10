namespace WebApi.Controllers;
using System.Threading.Tasks;

using Core.Contracts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NursingHomeController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public NursingHomeController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _uow.NursingHomeRepository.GetAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _uow.NursingHomeRepository.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }
}
