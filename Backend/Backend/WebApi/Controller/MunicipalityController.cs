namespace WebApi.Controllers;
using System.Threading.Tasks;

using Core.Contracts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MunicipalityController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public MunicipalityController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _uow.MunicipalityRepository.GetAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _uow.MunicipalityRepository.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }
}
