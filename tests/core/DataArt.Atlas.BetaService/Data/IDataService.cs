using System.Linq;

namespace DataArt.Atlas.BetaService.Data
{
    public interface IDataService
    {
        IQueryable<Sdk.Data> GetList();
    }
}
