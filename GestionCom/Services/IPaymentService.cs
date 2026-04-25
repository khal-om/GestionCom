using GestionCom.DTOs.Payment;

namespace GestionCom.Services
{
    public interface IPaymentService
    {
        Task AddPaymentAsync(CreatePaymentDto dto, int userId);
        Task UpdatePaymentReceivedStatusAsync(int paymentId, UpdatePaymentStatusDto dto, int userId);
    }
}
