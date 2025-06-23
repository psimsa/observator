using System;
using Observator.Generated;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Logging;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(ObservatorInfrastructure.ActivitySourceName)
    // The rest of your setup code goes here
    .AddConsoleExporter()
    .Build();

// See https://aka.ms/new-console-template for more information
Console.WriteLine("=== Observator Test App ===");
Console.WriteLine($"ActivitySource Name: {ObservatorInfrastructure.ActivitySourceName}");
Console.WriteLine($"ActivitySource Version: {ObservatorInfrastructure.Version}");
Console.WriteLine($"ActivitySource Instance: {ObservatorInfrastructure.ActivitySource}");
Console.WriteLine($"Meter Instance: {ObservatorInfrastructure.Meter}");

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var sampleLogger = loggerFactory.CreateLogger<TestApp.SampleService>();
var edgeLogger = loggerFactory.CreateLogger<TestApp.EdgeCases>();

var sampleService = new TestApp.SampleService(sampleLogger);
var greetResult = sampleService.Greet("World");
Console.WriteLine($"Greet result: {greetResult}");

var sampleService2 = new TestApp.SampleService(sampleLogger);
var greetResult2 = sampleService2.Greet("Universe");

var edgeCases = new TestApp.EdgeCases(edgeLogger);

Console.WriteLine("Test completed successfully!");

// Expression-bodied method
var greetExpr = sampleService.GreetExpression("EdgeCase");
Console.WriteLine($"GreetExpression result: {greetExpr}");

// Interface call
TestApp.ISampleService iface = sampleService;
var greetIface = iface.GreetViaInterface("Interface");
Console.WriteLine($"GreetViaInterface result: {greetIface}");

// Abstract/override call
TestApp.AbstractSampleService abs = sampleService;
var greetAbs = abs.GreetAbstract("Abstract");
Console.WriteLine($"GreetAbstract result: {greetAbs}");

// Exception handling
try
{
    var exceptionResult = sampleService.GreetWithException("Test");
}
catch (NotImplementedException ex)
{
    Console.WriteLine($"Caught expected exception: {ex.Message}");
}
