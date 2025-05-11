using System.Data.SqlClient;
using APBDTut9API.Models;

namespace APBDTut9API.Services;

public class ApiService : IApiService
{
    public readonly string _connectionString;

    public ApiService()
    {
        _connectionString =
            "Data Source=localhost, 1433; User=SA; Password=yourStrong()Password; Initial Catalog = master; Integrated Security=False;Connect Timeout=30;Encrypt=False";
    }
    public async Task<int> AddProductWarehouseAsync(DeliveryDTO delivery)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        if(delivery.IdProduct <= 0 || delivery.IdWarehouse <= 0) throw new Exception("Ids must be greater than zero");
        
        //checking if product with given id exists
        var checkProduct = new SqlCommand(
            "SELECT 1 FROM Product WHERE IdProduct = @IdProduct", connection);
        checkProduct.Parameters.AddWithValue("@IdProduct", delivery.IdProduct);
        var checkResult = await checkProduct.ExecuteScalarAsync();
        if(checkResult == null) throw new Exception("No product with such id found");

        //checking if warehouse with given id exists
        var checkWarehouse = new SqlCommand(
            "SELECT 1 FROM warehouse WHERE IdWarehouse = @IdWarehouse", connection);
        checkWarehouse.Parameters.AddWithValue("@IdWarehouse", delivery.IdWarehouse
        );
        var checkResultWarehouse = await checkWarehouse.ExecuteScalarAsync();
        if(checkResultWarehouse == null) throw new Exception("No warehouse with such id found");
        
        //checking if there is record in order table with such idproduct and amount
        var checkRecord = new SqlCommand(
            "SELECT IdOrder FROM [order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt", connection);
        checkRecord.Parameters.AddWithValue("@IdProduct", delivery.IdProduct);
        checkRecord.Parameters.AddWithValue("@Amount", delivery.Amount);
        checkRecord.Parameters.AddWithValue("@CreatedAt", delivery.CreatedAt);
        var orderId = (int?) await checkRecord.ExecuteScalarAsync();
        if(orderId == null) throw new Exception("No order that matches the request");
        
        //checking whether this order has not been completed
        var checkCompletion = new SqlCommand(
            "SELECT 1 FROM product_warehouse WHERE IdOrder = @IdOrder", connection);
        checkCompletion.Parameters.AddWithValue("@IdOrder", orderId);
        var checkResultCompletion = await checkCompletion.ExecuteScalarAsync();
        if(checkResultCompletion != null) throw new Exception("Order is already completed");
        
        //updating order date
        var updateOrderDate = new SqlCommand(
            "UPDATE [order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder", connection);
        updateOrderDate.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
        updateOrderDate.Parameters.AddWithValue("@IdOrder", orderId);
        await updateOrderDate.ExecuteNonQueryAsync();
        
        //getting price
        var getPrice = new SqlCommand(
            "SELECT Price FROM product WHERE IdProduct = @IdProduct", connection);
        getPrice.Parameters.AddWithValue("@IdProduct", delivery.IdProduct);
        var price = (decimal?) await getPrice.ExecuteScalarAsync();
        
        //inserting
        var insertRecord = new SqlCommand(
            "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) OUTPUT INSERTED.IdProductWarehouse VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)", connection);
        insertRecord.Parameters.AddWithValue("@IdWarehouse", delivery.IdWarehouse);
        insertRecord.Parameters.AddWithValue("@IdProduct", delivery.IdProduct);
        insertRecord.Parameters.AddWithValue("@IdOrder", orderId);
        insertRecord.Parameters.AddWithValue("@Amount", delivery.Amount);
        insertRecord.Parameters.AddWithValue("@Price", price * delivery.Amount);
        insertRecord.Parameters.AddWithValue("@CreatedAt", delivery.CreatedAt);
        
        var id = (int) await insertRecord.ExecuteScalarAsync();
        return id!;
    }

    public async Task<int> AddProductWarehouseProcedureAsync(DeliveryDTO delivery)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        if(delivery.IdProduct <= 0 || delivery.IdWarehouse <= 0) throw new Exception("Ids must be greater than zero");

        var procedure = new SqlCommand("AddProductToWarehouse", connection);
        procedure.CommandType = System.Data.CommandType.StoredProcedure;
        
        procedure.Parameters.AddWithValue("@IdProduct", delivery.IdProduct);
        procedure.Parameters.AddWithValue("@IdWarehouse", delivery.IdWarehouse);
        procedure.Parameters.AddWithValue("@Amount", delivery.Amount);
        procedure.Parameters.AddWithValue("@CreatedAt", delivery.CreatedAt);

        try
        {
            var id = await procedure.ExecuteScalarAsync();
            if (id == null) throw new Exception("Procedure didn't return anything");
            
            return Convert.ToInt32(id);
        }
        catch (Exception e)
        {
            throw new Exception("Something went wrong inside the procedure: " + e.Message);
        }
    }
}