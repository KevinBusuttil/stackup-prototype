using NUnit.Framework;

namespace StackUp.Tests
{
    public class PalletGridTests
    {
        private static PalletItem Item(string id, WeightClass w, StackClass s)
            => new PalletItem { SkuId = id, Weight = w, Stack = s };

        [Test]
        public void Places_and_counts()
        {
            var g = new PalletGrid(3, 3, 4);
            Assert.IsTrue(g.TryPlace(0, 0, Item("A", WeightClass.Medium, StackClass.Standard)));
            Assert.AreEqual(1, g.Count("A"));
            Assert.AreEqual(1, g.Height(0, 0));
            Assert.AreEqual(1, g.TotalItems);
        }

        [Test]
        public void Enforces_height_limit()
        {
            var g = new PalletGrid(1, 1, 2);
            var it = Item("A", WeightClass.Light, StackClass.Standard);
            Assert.IsTrue(g.TryPlace(0, 0, it));
            Assert.IsTrue(g.TryPlace(0, 0, it));
            Assert.IsFalse(g.TryPlace(0, 0, it), "third item should exceed the height limit");
        }

        [Test]
        public void Blocks_heavy_on_fragile_only()
        {
            var g = new PalletGrid(1, 1, 4);
            Assert.IsTrue(g.TryPlace(0, 0, Item("glass", WeightClass.Light, StackClass.Fragile)));
            Assert.IsFalse(g.CanPlace(0, 0, Item("steel", WeightClass.Heavy, StackClass.Standard), out _),
                "heavy must not stack on fragile");
            Assert.IsTrue(g.CanPlace(0, 0, Item("box", WeightClass.Medium, StackClass.Standard), out _),
                "non-heavy may stack on fragile");
        }

        [Test]
        public void FindFirstValidCell_spreads_across_columns()
        {
            var g = new PalletGrid(2, 1, 4);
            var it = Item("A", WeightClass.Light, StackClass.Standard);
            Assert.IsTrue(g.FindFirstValidCell(it, out int x0, out int z0));
            g.TryPlace(x0, z0, it);
            Assert.IsTrue(g.FindFirstValidCell(it, out int x1, out int z1));
            Assert.IsFalse(x0 == x1 && z0 == z1, "second item should prefer the empty column");
        }

        [Test]
        public void Out_of_bounds_is_rejected()
        {
            var g = new PalletGrid(2, 2, 2);
            Assert.IsFalse(g.CanPlace(5, 0, Item("A", WeightClass.Light, StackClass.Standard), out _));
        }
    }
}
