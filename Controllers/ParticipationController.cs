using AlgorandKMDServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgorandKMDServer.Controllers
{
    /// <summary>
    /// Better name for the service is participation server as kmd is used in different way
    /// </summary>
    [ApiController]
    [Route("/v1/participation")]
    public class ParticipationController : KMDController
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="participationConfiguration"></param>
        public ParticipationController(ILogger<KMDController> logger, IConfiguration configuration, IOptionsMonitor<ParticipationConfiguration> participationConfiguration) : base(logger, configuration, participationConfiguration)
        {
        }
    }
}
