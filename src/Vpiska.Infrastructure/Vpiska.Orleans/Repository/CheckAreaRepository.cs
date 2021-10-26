using System.Linq;
using System.Threading.Tasks;
using Vpiska.Domain.EventAggregate.Repository;

namespace Vpiska.Orleans.Repository
{
    internal sealed class CheckAreaRepository : ICheckAreaRepository
    {
        private readonly AreaSettings _settings;
        
        public CheckAreaRepository(AreaSettings settings)
        {
            _settings = settings;
        }

        public Task<bool> IsExist(string areaName) => Task.FromResult(_settings.Areas.Contains(areaName));
    }
}