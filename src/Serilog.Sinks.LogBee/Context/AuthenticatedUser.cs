namespace Serilog.Sinks.LogBee.Context
{
    public class AuthenticatedUser
    {
        public string Name { get; init; }

        public AuthenticatedUser(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }
    }
}
