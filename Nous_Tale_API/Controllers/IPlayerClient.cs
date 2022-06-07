using Nous_Tale_API.Model.Entities;
using Nous_Tale_API.Model.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nous_Tale_API.Controllers
{
    /// <summary>
    /// Defines the methods in the client available to be called.
    /// </summary>
    public interface IPlayerClient
    {
        // Room methods
        Task PlayerExited(int playerID, int hostPlayerID);
        Task PlayerEntered(Player player);
        Task ReadyCountChanged(int readyCount);
        Task TalesCreated(List<TaleVM> newTales);

        Task TaleWasUpdated(TaleVM updatedTale, int index);
        Task EveryoneIsReady();
    }
}
