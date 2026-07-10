namespace WebApi.Controllers;
using System.Threading.Tasks;

using Core.Contracts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PostalCodeController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public PostalCodeController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _uow.PostalCodeRepository.GetAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _uow.PostalCodeRepository.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }
}
