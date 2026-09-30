using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.Floors;

public sealed record FloorPersistenceResult
{
	private FloorPersistenceResult(FloorSaveStatus status, Floor? floor)
	{
		Status = status;
		Floor = floor;
	}

	public FloorSaveStatus Status { get; }

	public Floor? Floor { get; }

	public static FloorPersistenceResult Succeeded(Floor floor) =>
		new(FloorSaveStatus.Success, floor);

	public static FloorPersistenceResult MissingFloor() =>
		new(FloorSaveStatus.FloorNotFound, null);

	public static FloorPersistenceResult MissingBuilding() =>
		new(FloorSaveStatus.BuildingNotFound, null);
}