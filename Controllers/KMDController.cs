using AlgorandAuthentication;
using AlgorandKMDServer.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AlgorandKMDServer.Controllers
{
    /// <summary>
    /// KMD controller
    /// </summary>

    [ApiController]
    [Route("/v1/[controller]")]
    public class KMDController : ControllerBase
    {
        /// <summary>
        /// Last run of the addpartkey method. This method is cpu sensitive and might cause DDOS if users are performing too fast requests.
        /// </summary>
        private static DateTimeOffset LastRun = DateTimeOffset.Now;
        private static Status? LastStats = null;
        private readonly ILogger<KMDController> _logger;
        private readonly int LockTime = 30;
        private readonly int MaximumRounds = 1000000;
        private readonly string Realm = "";
        private readonly string Network = "";
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">DI logger</param>
        /// <param name="configuration">DI configuration</param>
        public KMDController(ILogger<KMDController> logger, IConfiguration configuration)
        {
            _logger = logger;
            if (int.TryParse(configuration["LockTime"], out var time))
            {
                LockTime = time;
            }
            if (int.TryParse(configuration["MaximumRounds"], out var rounds))
            {
                MaximumRounds = rounds;
            }
            Realm = configuration["algod:realm"] ?? throw new Exception("Auth realm is not defined");
            Network = configuration["algod:networkGenesisId"] ?? throw new Exception("Auth network is not defined");

        }

        /// <summary>
        /// Shows the configured account for this 2FA system
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetRealm")]
        public ActionResult<string> GetRealm()
        {
            return Ok(Realm);
        }
        /// <summary>
        /// Shows the configured account for this 2FA system
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetNetwork")]
        public ActionResult<string> GetNetwork()
        {
            return Ok(Network);
        }
        /// <summary>
        /// Show current kmd status
        /// </summary>
        /// <returns></returns>

        [ProducesResponseType(typeof(Status), 200)]
        [ProducesResponseType(401)] // unauthorized
        [ProducesResponseType(503)] // server down .. from k8s cluster routing
        [HttpGet("status")]
        public ActionResult<Status> Status()
        {
#if !DEBUG
            if (LastStats != null && LastStats.Time.AddMinutes(10) > DateTimeOffset.Now)
            {
                return LastStats;
            }

            System.Diagnostics.Process process = new();
            System.Diagnostics.ProcessStartInfo startInfo = new()
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = "goal",
                Arguments = $"account listpartkeys",
                RedirectStandardOutput = true
            };
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();

            var ret = process.StandardOutput.ReadToEnd();
#else
            var ret = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("UmVnaXN0ZXJlZCAgQWNjb3VudCAgICAgIFBhcnRpY2lwYXRpb25JRCAgIExhc3QgVXNlZCAgRmlyc3Qgcm91bmQgIExhc3Qgcm91bmQKbm8gICAgICAgICAgU0NIVS4uLklFV0EgIFc1QUgySVZHLi4uICAgICAgICAgICAgIE4vQSAgICAgMjM0NDUwMDAgICAgMjQ0NDUwMDAKbm8gICAgICAgICAgU0NIVS4uLklFV0EgIFFGMkNYWlBPLi4uICAgICAgICAgICAgIE4vQSAgICAgMjM0NDUwMDAgICAgMjQ0NDUwMDAKbm8gICAgICAgICAgU0NIVS4uLklFV0EgIDdKS0hPN0lZLi4uICAgICAgICAgICAgIE4vQSAgICAgMjI2NzMyOTEgICAgMjM2NzMyOTEKbm8gICAgICAgICAgU0NIVS4uLklFV0EgIE5LREZKWU5OLi4uICAgICAgICAgICAgIE4vQSAgICAgMjI2NzMyOTEgICAgMjM2NzMyOTEKbm8gICAgICAgICAgU0NIVS4uLklFV0EgIEFNWE42MjQ3Li4uICAgICAgICAgICAgIE4vQSAgICAgMjM0NDQ5MjkgICAgMjQ0NDQ5MjkK"));
#endif
            //Console.WriteLine(ret);
            //Console.WriteLine(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ret)));

            var stats = new Status();
            foreach (var line in ret.Split("\n"))
            {
                if (string.IsNullOrEmpty(line)) continue;
                var lineTrimed = line.Replace("  ", " ");
                lineTrimed = lineTrimed.Replace("  ", " ");
                lineTrimed = lineTrimed.Replace("  ", " ");
                lineTrimed = lineTrimed.Replace("  ", " ");
                lineTrimed = lineTrimed.Replace("  ", " ");
                var data = lineTrimed.Split(" ");
                if (data.Length == 6)
                {
                    var active = false;
                    if (data[0] == "yes")
                    {
                        // active
                        active = true;
                    }

                    if (!long.TryParse(data[4], out var firstRound))
                    {
                        continue;
                    }

                    if (!long.TryParse(data[5], out var lastRound))
                    {
                        continue;
                    }

                    if (active)
                    {
                        stats.ActiveKeys++;
                        stats.ActiveKeysRoundsSum += lastRound - firstRound;
                    }
                    stats.AllKeysCount++;
                    stats.AllKeysRoundsSum += lastRound - firstRound;
                }
            }
            stats.Time = DateTimeOffset.Now;
            LastStats = stats;
            Console.WriteLine($"Stats: {DateTimeOffset.Now} {JsonConvert.SerializeObject(stats)}");
            return Ok(stats);
        }

        /// <summary>
        /// Creates participation key for this server. User can sign the request to use this participation key.
        /// 
        /// This method is cpu sensitive and might cause DDOS if users are performing too fast requests. It is secured with limit of 1000000 rounds, and 10 second global call limit.
        /// </summary>
        /// <param name="roundFirstValid">First valid round for participation</param>
        /// <param name="roundLastValid">Last valid round for participation</param>
        /// <param name="address"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AlgorandAuthenticationHandler.ID)]
        [ProducesResponseType(typeof(ParticipationKey), 200)]
        [HttpGet("addpartkey")]
        public ActionResult<ParticipationKey> Addpartkey(int roundFirstValid, int roundLastValid, string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                {
                    address = User?.Identity?.Name ?? "";
                }
                //var address = User?.Identity?.Name;
                string message = $"{DateTimeOffset.Now} addpartkey requested {User?.Identity?.Name} {address} {roundFirstValid} {roundLastValid}";
                _logger?.LogInformation(message);
                if (LastRun.AddSeconds(LockTime) > DateTimeOffset.Now)
                {
                    throw new Exception($"Server is busy. Please try again in {LockTime} seconds.");
                }
                LastRun = DateTimeOffset.Now;
                if (string.IsNullOrEmpty(address))
                {
                    throw new Exception("Invalid authentization");
                }

                if (!Algorand.Address.IsValid(address))
                {
                    throw new Exception("Address is invalid");
                }

                if (roundLastValid <= roundFirstValid) throw new Exception("roundLastValid is lower than roundFirstValid");
                if (roundLastValid - roundFirstValid > MaximumRounds) throw new Exception($"You can create part keys max for {MaximumRounds} rounds");
                System.Diagnostics.Process process = new();
                System.Diagnostics.ProcessStartInfo startInfo = new()
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = "goal",
                    Arguments = $"account addpartkey -a {address} --roundFirstValid {roundFirstValid} --roundLastValid {roundLastValid}",
                    RedirectStandardOutput = true
                };
                process.StartInfo = startInfo;

                process.Start();
                process.WaitForExit();

                var ret = process.StandardOutput.ReadToEnd();
                var search = "key generation successful. Participation ID: ";
                var index = ret.IndexOf(search);
                if (index <= 0) throw new Exception("Error occured while generating participation key 0x001");
                var trim = ret[(index + search.Length)..];
                var indexEnd = trim.IndexOf("\n");
                if (index <= 0) throw new Exception("Error occured while generating participation key 0x002");
                var partKey = trim[..indexEnd].Trim();

                System.Diagnostics.Process process2 = new();
                System.Diagnostics.ProcessStartInfo startInfo2 = new()
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = "goal",
                    Arguments = $"account partkeyinfo",
                    RedirectStandardOutput = true
                };
                process2.StartInfo = startInfo2;

                process2.Start();
                process2.WaitForExit();
                var process2Output = process2.StandardOutput.ReadToEnd();

                Console.WriteLine(process2Output);
                Console.WriteLine(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(process2Output)));

                ParticipationKey participationKey = new();
                foreach (var line in process2Output.Split("\n"))
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    var data = line.Split(":");
                    if (data.Length != 2) continue;
                    if (data[0].StartsWith("Participation ID"))
                    {
                        if (participationKey.ParticipationId == partKey)
                        {
                            // previous processed item was the id we wanted

                            return participationKey;
                        }
                        participationKey = new ParticipationKey()
                        {
                            ParticipationId = data[1].Trim(),
                        };
                    }

                    if (data[0].StartsWith("Parent address"))
                    {
                        participationKey.ParentAddress = data[1].Trim();
                    }
                    if (data[0].StartsWith("Selection key"))
                    {
                        participationKey.SelectionKey = data[1].Trim();
                    }

                    if (data[0].StartsWith("Voting key"))
                    {
                        participationKey.VoteKey = data[1].Trim();
                    }

                    if (data[0].StartsWith("State proof key"))
                    {
                        participationKey.StateProofKey = data[1].Trim();
                    }

                    if (data[0].StartsWith("First round"))
                    {
                        if (ulong.TryParse(data[1].Trim(), out var num))
                        {
                            participationKey.FirstRound = num;
                        }
                    }
                    if (data[0].StartsWith("Last round"))
                    {
                        if (ulong.TryParse(data[1].Trim(), out var num))
                        {
                            participationKey.LastRound = num;
                        }
                    }
                    if (data[0].StartsWith("Key dilution"))
                    {
                        if (ulong.TryParse(data[1].Trim(), out var num))
                        {
                            participationKey.VoteKeyDilution = num;
                        }
                    }

                }

                if (participationKey.ParticipationId == partKey)
                {
                    // previous processed item was the id we wanted

                    return participationKey;
                }

                Console.WriteLine(ret);
                return Ok(partKey);
            }
            catch (Exception exc)
            {
                return BadRequest(new ProblemDetails() { Detail = exc.Message });
            }
        }
    }
}