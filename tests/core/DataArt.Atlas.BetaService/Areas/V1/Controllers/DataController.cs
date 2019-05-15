using System.Linq;
using DataArt.Atlas.BetaService.Data;
using DataArt.Atlas.Core.Application.Http.OData;
using Microsoft.AspNetCore.Mvc;

namespace DataArt.Atlas.BetaService.Areas.V1.Controllers
{
    [Route("api/v1/data")]
    public class DataController : ControllerBase
    {
        private readonly IDataService service;

        public DataController(IDataService service)
        {
            this.service = service;
        }

        [HttpGet]
        [Route("")]
        [EnableQueryWithInlineCount]
        public IQueryable<Sdk.Data> GetData()
        {
            return service.GetList();
        }
    }
}
