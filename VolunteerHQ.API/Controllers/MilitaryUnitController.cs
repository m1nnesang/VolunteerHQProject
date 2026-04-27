using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolunteerHQ.Core.DTOs.MilitaryDTOs;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MilitaryUnitController : ControllerBase
{
    private readonly IMilitaryUnitService _militaryUnitService;
    
    public MilitaryUnitController(IMilitaryUnitService militaryUnitService)
    {
        _militaryUnitService = militaryUnitService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(RegisterMilitaryUnitDto dto)
    {
        var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _militaryUnitService.CreateUnit(dto , adminId);
        
        return Ok(result);
    }

    [HttpPost ("login")]
    public async Task<IActionResult> Login(LogMilitaryUnitDto dto)
    {
        var result = await _militaryUnitService.Login(dto);
        
        return Ok(result);
    }

    [HttpGet("{unitId}")]
    public async Task<IActionResult> GetUnit(int unitId)
    {
        var userId = User.Identity?.IsAuthenticated == true 
            ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!) 
            : (int?)null;
        
        var result = await _militaryUnitService.GetUnit(unitId, userId);
        
        return Ok(result);
    }
}