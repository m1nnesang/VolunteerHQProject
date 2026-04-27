namespace VolunteerHQ.Core.DTOs.MilitaryDTOs;

public record RegisterMilitaryUnitDto(string Login,string Password, string UnitName, string ContactPerson, bool IsNameHidden);