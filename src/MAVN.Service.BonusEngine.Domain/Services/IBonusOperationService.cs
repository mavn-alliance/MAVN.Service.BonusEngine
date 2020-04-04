using System.Threading.Tasks;
using MAVN.Service.BonusEngine.Domain.Models;

namespace MAVN.Service.BonusEngine.Domain.Services
{
    public interface IBonusOperationService
    {
        Task AddBonusOperationAsync(BonusOperation bonusOperation);
    }
}
