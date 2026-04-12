using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Infrastructure.Services;

namespace VolunteerHQ.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly NovaPoshtaService _novaPoshtaService;

    public CitiesController(NovaPoshtaService novaPoshtaService)
    {
        _novaPoshtaService = novaPoshtaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCities()
    {
        var city = await _novaPoshtaService.GetCity();

        return Ok(city);
    }
}