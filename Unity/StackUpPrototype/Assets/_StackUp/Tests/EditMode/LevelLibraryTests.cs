using NUnit.Framework;

namespace StackUp.Tests
{
    public class LevelLibraryTests
    {
        [Test]
        public void Has_eight_campaign_levels_with_names()
        {
            Assert.AreEqual(8, LevelLibrary.CampaignCount);
            for (int i = 0; i < LevelLibrary.CampaignCount; i++)
                Assert.IsFalse(string.IsNullOrEmpty(LevelLibrary.Get(i).Name), $"level {i} needs a name");
        }

        [Test]
        public void Get_clamps_out_of_range_to_first()
        {
            Assert.AreSame(LevelLibrary.Campaign[0], LevelLibrary.Get(-1));
            Assert.AreSame(LevelLibrary.Campaign[0], LevelLibrary.Get(999));
        }

        [Test]
        public void Difficulty_progression_flags()
        {
            Assert.IsFalse(LevelLibrary.Get(0).UseStacking, "First Pick is pick-and-load only");
            Assert.IsTrue(LevelLibrary.Get(2).UseStacking, "Stacking Intro introduces the pallet");
            Assert.IsTrue(LevelLibrary.Get(3).UseVerification, "Verification level requires verifying");
            Assert.Greater(LevelLibrary.Get(7).MaxConcurrent, 1, "Controlled Chaos runs concurrent orders");
        }

        [Test]
        public void Endless_config_is_endless()
        {
            var e = LevelLibrary.Endless();
            Assert.IsTrue(e.Endless);
            Assert.IsTrue(e.UseStacking && e.UseVerification);
        }
    }
}
