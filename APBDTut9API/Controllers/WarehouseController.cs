namespace APBDTut9API.Controllers;

using APBDTut9API.Models;
using APBDTut9API.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IApiService _service;

    public WarehouseController(IApiService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductWarehouseAsync(DeliveryDTO delivery)
    {
        try
        {
            var id = await _service.AddProductWarehouseAsync(delivery);
            return Created("", id);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("procedure")]
    public async Task<IActionResult> AddProductWarehouseProcedureAsync(DeliveryDTO delivery)
    {
        try
        {
            var id = await _service.AddProductWarehouseProcedureAsync(delivery);
            return Created("", id);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}