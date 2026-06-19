using NUnit.Framework;

namespace StackUp.Tests
{
    public class ToteInventoryTests
    {
        [Test]
        public void Adds_up_to_capacity_and_reports_overflow()
        {
            var t = new ToteInventory { MaxUnits = 3 };
            Assert.AreEqual(2, t.Add("A", 2));
            Assert.AreEqual(2, t.UnitCount);
            Assert.AreEqual(1, t.Add("A", 5), "only one unit of space remained");
            Assert.IsTrue(t.IsFull);
            Assert.AreEqual(3, t.GetQuantity("A"));
        }

        [Test]
        public void Removes_and_clears()
        {
            var t = new ToteInventory { MaxUnits = 10 };
            t.Add("A", 4);
            Assert.AreEqual(3, t.Remove("A", 3));
            Assert.AreEqual(1, t.GetQuantity("A"));
            Assert.AreEqual(1, t.Remove("A", 9), "cannot remove more than present");
            Assert.AreEqual(0, t.GetQuantity("A"));
            Assert.IsFalse(t.Contents.ContainsKey("A"), "empty SKUs are dropped");
        }

        [Test]
        public void Rejects_invalid_input()
        {
            var t = new ToteInventory { MaxUnits = 5 };
            Assert.AreEqual(0, t.Add(null, 2));
            Assert.AreEqual(0, t.Add("A", 0));
            Assert.AreEqual(0, t.Remove("A", 1));
        }
    }
}
