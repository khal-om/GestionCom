namespace GestionCom.Services
{
    public interface IStockService
    {
        Task<int> GetStockAsync(int produitId);
    }
}
