namespace APBDTut9API.Models;

public class DeliveryDTO
{
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}