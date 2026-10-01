namespace SmartBuilding.Application.Users;

public interface IPasswordHashService
{
    string HashPassword(string password);
}