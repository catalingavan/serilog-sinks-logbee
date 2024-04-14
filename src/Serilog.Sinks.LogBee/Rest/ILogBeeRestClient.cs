namespace Serilog.Sinks.LogBee.Rest
{
    internal interface ILogBeeRestClient
    {
        void CreateRequestLog(CreateRequestLogPayload payload);
    }
}
