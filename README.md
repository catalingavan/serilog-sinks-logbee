# Sinks

A collection of Serilog sinks that write events to [logBee.net](https://logbee.net).

### [Serilog.Sinks.LogBee](src/Serilog.Sinks.LogBee#readme)

A Serilog sink used for non-web applications (Console applications, Worker services).

Examples:

- [ConsoleApp/Program1.cs](samples/Serilog.Sinks.LogBee_ConsoleApp/Program1.cs): Simple usage

- [ConsoleApp/Program2.cs](samples/Serilog.Sinks.LogBee_ConsoleApp/Program2.cs): Custom configuration

- [ConsoleApp/Program3.cs](samples/Serilog.Sinks.LogBee_ConsoleApp/Program3.cs) Using Microsoft.Extensions.Logging and Microsoft.Extensions.DependencyInjection

- [ConsoleApp/Program4.cs](samples/Serilog.Sinks.LogBee_ConsoleApp/Program4.cs): A console application which runs periodically

- [WorkerService](samples/Serilog.Sinks.LogBee_WorkerService/): A worker service application

### [Serilog.Sinks.LogBee.AspNetCore](src/Serilog.Sinks.LogBee.AspNetCore#readme)

A Serilog sink used for web applications.

Examples:

 - [WebApp](samples/Serilog.Sinks.LogBee_WebApp/)