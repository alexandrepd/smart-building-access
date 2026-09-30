namespace SmartBuilding.Application.Floors;

public sealed record FloorSaveResult
{
	private FloorSaveResult(FloorSaveStatus status, FloorDto? floor)
	{
		Status = status;
		Floor = floor;
	}

	public FloorSaveStatus Status { get; }

	public FloorDto? Floor { get; }

	public static FloorSaveResult Succeeded(FloorDto floor) =>
		new(FloorSaveStatus.Success, floor);

	public static FloorSaveResult MissingFloor() =>
		new(FloorSaveStatus.FloorNotFound, null);

	public static FloorSaveResult MissingBuilding() =>
		new(FloorSaveStatus.BuildingNotFound, null);
}