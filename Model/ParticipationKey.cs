namespace AlgorandKMDServer.Model
{
    /// <summary>
    /// Return of the create part key method
    /// </summary>
    public class ParticipationKey
    {
        /// <summary>
        /// Id
        /// </summary>
        public string ParticipationId { get; set; } = "";
        /// <summary>
        /// Address
        /// </summary>
        public string ParentAddress { get; set; } = "";
        /// <summary>
        /// SelectionKey
        /// </summary>
        public string SelectionKey { get; set; } = "";
        /// <summary>
        /// Voting key
        /// </summary>
        public string VoteKey { get; set; } = "";
        /// <summary>
        /// State proof key
        /// </summary>
        public string StateProofKey { get; set; } = "";
        /// <summary>
        /// First round
        /// </summary>
        public ulong FirstRound { get; set; } = 0;
        /// <summary>
        /// Last round
        /// </summary>
        public ulong LastRound { get; set; } = 0;
        /// <summary>
        /// Key dilution
        /// </summary>
        public ulong VoteKeyDilution { get; set; } = 0;
    }
}
