using System.Collections.Generic;
using System.Threading.Tasks;
using DataArt.Atlas.Infrastructure.Interfaces;
using DataArt.Atlas.Infrastructure.OData;

namespace DataArt.Atlas.BetaService.Sdk
{
    public interface IClient : ISdkClient
    {
        Task<ODataResponse<Data>> GetListAsync(IDictionary<string, string> oDataQuery);
    }
}
