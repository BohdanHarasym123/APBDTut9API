using APBDTut9API.Models;

namespace APBDTut9API.Services;

public interface IApiService
{
    Task<int> AddProductWarehouseAsync(DeliveryDTO delivery);
    
    Task<int> AddProductWarehouseProcedureAsync(DeliveryDTO delivery);
}