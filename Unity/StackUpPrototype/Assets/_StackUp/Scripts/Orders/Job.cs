namespace StackUp
{
    public enum JobType
    {
        Pick,
        Stack,
        Verify,
        Load,
        Rework
    }

    public enum JobState
    {
        Pending,
        Active,
        Completed,
        Failed
    }

    /// <summary>
    /// A unit of work derived from an order (pick, stack, verify, load, rework).
    /// </summary>
    public class Job
    {
        public string JobId;
        public JobType Type;
        public string OrderId;
        public string SkuId;
        public int Quantity;
        public string SourceSlotId;
        public string TargetLocationId;
        public JobState State = JobState.Pending;
    }
}
