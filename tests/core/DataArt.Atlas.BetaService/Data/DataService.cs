using System.Linq;

namespace DataArt.Atlas.BetaService.Data
{
    internal class DataService : IDataService
    {
        private static readonly Sdk.Data[] Data =
        {
            new Sdk.Data
            {
                Id = 1,
                Name = "z"
            },
            new Sdk.Data
            {
                Id = 2,
                Name = "f"
            },
            new Sdk.Data
            {
                Id = 3,
                Name = "t"
            }
        };

        public IQueryable<Sdk.Data> GetList()
        {
            return Data.AsQueryable();
        }
    }
}
