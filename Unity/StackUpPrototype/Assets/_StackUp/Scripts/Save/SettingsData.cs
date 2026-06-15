namespace StackUp
{
    /// <summary>
    /// Player settings persisted to settings.json. UI scale supports Steam Deck
    /// readability (Section 18.3); volumes drive the audio mix.
    /// </summary>
    [System.Serializable]
    public class SettingsData
    {
        public float UiScale = 1f;        // 1.0 .. 1.6
        public float MasterVolume = 1f;   // 0 .. 1
        public float MusicVolume = 0.7f;
        public float SfxVolume = 1f;
    }
}
