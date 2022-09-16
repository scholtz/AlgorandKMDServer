namespace AlgorandKMDServer.Model
{
    /// <summary>
    /// Status of the kmd server
    /// </summary>
    public class Status
    {
        /// <summary>
        /// Time of the snapshot
        /// </summary>
        public DateTimeOffset Time { get; set; }
        /// <summary>
        /// Active participation keys
        /// </summary>
        public long ActiveKeys { get; set; }
        /// <summary>
        /// Sum of last round minus first round of all active keys
        /// </summary>
        public long ActiveKeysRoundsSum { get; set; }
        /// <summary>
        /// Count of generated participation keys
        /// </summary>
        public long AllKeysCount { get; set; }
        /// <summary>
        /// Sum of last round minus first round of all keys
        /// </summary>
        public long AllKeysRoundsSum { get; set; }
    }
}
