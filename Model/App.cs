namespace AlgorandKMDServer.Model
{
    /// <summary>
    /// For version controller
    /// </summary>
    public class App
    {
        /// <summary>
        /// Identifies specific run of the application
        /// </summary>
        public readonly static string InstanceId = Guid.NewGuid().ToString();
        /// <summary>
        /// Identifies specific run of the application
        /// </summary>
        public readonly static DateTimeOffset Started = DateTimeOffset.Now;
    }
}
