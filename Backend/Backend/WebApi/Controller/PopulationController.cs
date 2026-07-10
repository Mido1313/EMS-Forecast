namespace WebApi.Controllers;
using System.Threading.Tasks;

using Core.Contracts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PopulationController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public PopulationController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _uow.PopulationRepository.GetAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _uow.PopulationRepository.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }
}
