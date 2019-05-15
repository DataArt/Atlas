using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClientService.Areas.V1.Controllers
{
    [Route("api/v1/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly ILogger<JobsController> logger;

        public JobsController(ILogger<JobsController> logger)
        {
            this.logger = logger;
        }

        [Route("")]
        [HttpPost]
        public void Work()
        {
            logger.LogInformation("Work completed");
        }
    }
}
