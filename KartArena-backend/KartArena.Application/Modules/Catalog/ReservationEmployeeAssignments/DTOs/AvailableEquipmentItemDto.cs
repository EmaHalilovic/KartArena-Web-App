namespace KartArena.Application.Features.ReservationEmployeeAssignments.DTOs;

public class AvailableEquipmentItemDto
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public int EquipmentTypeId { get; set; }
    public string EquipmentTypeName { get; set; } = string.Empty;
}
