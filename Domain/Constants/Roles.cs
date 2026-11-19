
namespace HotelManagement.Domain.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Receptionist = "Receptionist";
    public const string Housekeeper = "Housekeeper";
    public const string Customer = "Customer";

    
    public const string AdminOrManager = Admin + "," + Manager;
    public const string Staff = Admin + "," + Manager + "," + Receptionist + "," + Housekeeper;

   
    public const string FrontDesk = Admin + "," + Manager + "," + Receptionist;

   
    public const string CustomerOrFrontDesk = Customer + "," + FrontDesk;

    
    public static readonly string[] All = [Admin, Manager, Receptionist, Housekeeper, Customer];

  
    public static readonly string[] Assignable = [Manager, Receptionist, Housekeeper];
}