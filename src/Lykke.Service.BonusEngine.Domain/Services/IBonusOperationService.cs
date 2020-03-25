using System.Threading.Tasks;
using Lykke.Service.BonusEngine.Domain.Models;

namespace Lykke.Service.BonusEngine.Domain.Services
{
    public interface IBonusOperationService
    {
        Task AddBonusOperationAsync(BonusOperation bonusOperation);
    }
}
