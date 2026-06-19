using NUnit.Framework;

namespace StackUp.Tests
{
    public class ProgressDataTests
    {
        [Test]
        public void GetOrCreate_returns_same_record()
        {
            var p = new ProgressData();
            var a = p.GetOrCreate(2);
            a.BestScore = 50;
            var b = p.GetOrCreate(2);
            Assert.AreSame(a, b);
            Assert.AreEqual(1, p.Levels.Count);
            Assert.AreEqual(50, b.BestScore);
        }

        [Test]
        public void GetOrCreate_adds_distinct_levels()
        {
            var p = new ProgressData();
            p.GetOrCreate(0);
            p.GetOrCreate(1);
            p.GetOrCreate(0);
            Assert.AreEqual(2, p.Levels.Count);
        }
    }
}
