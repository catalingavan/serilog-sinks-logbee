using System;

namespace Serilog.Sinks.LogBee.ContextProperties
{
    public class IntegrationClient
    {
        public string Name { get; private set; }
        public Version Version { get; private set; }

        public IntegrationClient(string name, Version version)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }
    }
}
