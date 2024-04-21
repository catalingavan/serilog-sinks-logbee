using System;

namespace Serilog.Sinks.LogBee.ContextProperties
{
    public class AuthenticatedUser
    {
        public string Name { get; private set; }

        public AuthenticatedUser(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }
    }
}
