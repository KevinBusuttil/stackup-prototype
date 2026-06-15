using System.Collections.Generic;

namespace StackUp
{
    /// <summary>Outcome of a verification check, used for UI feedback (Section 15).</summary>
    public class VerificationResult
    {
        public class Line
        {
            public string SkuId;
            public int Required;
            public int Collected;
            public int Missing;
        }

        public string OrderId;
        public bool Passed;
        public int WrongItems;
        public List<Line> Lines = new List<Line>();
    }
}
