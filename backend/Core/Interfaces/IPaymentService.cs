using AutoServ.Core.Models;

namespace AutoServ.Core.Interfaces
{
    public interface IPaymentService
    {
        //  data та signature для відправки на фронтенд
        Result<(string Data, string Signature)> GeneratePaymentData(int orderId, decimal amount, string description);

        Result<int> ProcessCallback(string data, string signature);
    }
}