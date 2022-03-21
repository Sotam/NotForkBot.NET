namespace PokeNX.Core.Models
{
    public class GameOffset
    {
        public string BuildId { get; }

        public string Version { get; }

        public uint PlayerPrefsProviderInstance { get; }

        public string MainPointer { get; }

        public GameOffset(string buildId, string version, uint playerPrefsProviderInstance, string mainPointer)
        {
            BuildId = buildId;
            Version = version;
            PlayerPrefsProviderInstance = playerPrefsProviderInstance;
            MainPointer = mainPointer;
        }
    }
}
