namespace AlgorandKMDServer.Model
{
    /// <summary>
    /// App configuration
    /// </summary>
    public class ParticipationConfiguration
    {
        /// <summary>
        /// How much time is the app locked in order to create new participation keys. Prevention agains DDOS attacks
        /// </summary>
        public int LockTime { get; set; } = 120;
        /// <summary>
        /// Maximum rounds
        /// </summary>
        public int MaximumRounds { get; set; } = 1000000;
    }
}
