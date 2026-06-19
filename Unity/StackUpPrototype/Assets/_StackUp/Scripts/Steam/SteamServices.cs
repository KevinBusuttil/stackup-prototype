namespace StackUp
{
    /// <summary>
    /// Service locator for the active <see cref="ISteamService"/>. Defaults to the
    /// mock so the game runs without Steam; M6 swaps in a real implementation by
    /// calling <see cref="Init"/> with it before anything else uses the service.
    /// </summary>
    public static class SteamServices
    {
        public static ISteamService Current { get; private set; }

        public static void Init(ISteamService service = null)
        {
            if (Current != null) return;

            var svc = service ?? new MockSteamService();
            if (!svc.Initialize() && !(svc is MockSteamService))
            {
                // A real service (e.g. Steamworks) failed to start — fall back to the mock
                // so the game still runs (Steam not installed / not running / no app id).
                svc = new MockSteamService();
                svc.Initialize();
            }
            Current = svc;
        }

        public static void Shutdown()
        {
            Current?.Shutdown();
            Current = null;
        }
    }
}
