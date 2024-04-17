namespace Serilog.Sinks.LogBee.Context
{
    public class IntegrationClient
    {
        public string Name { get; init; }
        public Version Version { get; init; }

        public IntegrationClient(string name, Version version)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }
    }
}
