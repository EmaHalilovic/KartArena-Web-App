namespace KartArena.Application.Features.ReservationEmployeeAssignments.DTOs;

public class ReservationEmployeeAssignmentDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int? EquipmentItemId { get; set; }
    public string? EquipmentItemName { get; set; }
    public int? EquipmentTypeId { get; set; }
    public string? EquipmentCategoryName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
