using AlgorandKMDServer.Extension;
using AlgorandKMDServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgorandKMDServer.Controllers
{
    /// <summary>
    /// This controller returns version of the current api
    /// </summary>
    [ApiController]
    [Route("v1")]
    public class VersionController : ControllerBase
    {
        private readonly IOptionsMonitor<ParticipationConfiguration> participationConfiguration;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="participationConfiguration"></param>
        public VersionController(IOptionsMonitor<ParticipationConfiguration> participationConfiguration)
        {
            this.participationConfiguration = participationConfiguration;
        }
        /// <summary>
        /// Returns version of the current api
        /// 
        /// For development purposes it returns version of assembly, for production purposes it returns string build by pipeline which contains project information, pipeline build version, assembly version, and build date
        /// </summary>
        /// <returns></returns>
        [HttpGet("version")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public Model.Version Get()
        {
            var ret = VersionExtensions.GetVersion(
                App.InstanceId,
                App.Started,
                GetType()?.Assembly?.GetName()?.Version?.ToString()
            );
            return ret;
        }
        /// <summary>
        /// Returns version of the current api
        /// 
        /// For development purposes it returns version of assembly, for production purposes it returns string build by pipeline which contains project information, pipeline build version, assembly version, and build date
        /// </summary>
        /// <returns></returns>
        [HttpGet("config")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ParticipationConfiguration GetConfig()
        {
            return participationConfiguration.CurrentValue;
        }
    }
}
