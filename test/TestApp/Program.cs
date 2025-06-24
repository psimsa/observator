using System;
using Observator.Generated.TestApp;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Logging;
using TestLib;
using System.Diagnostics;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(ObservatorInfrastructure.ActivitySourceName)
    // The rest of your setup code goes here
    .AddConsoleExporter()
    .Build();
var acs = new ActivitySource(ObservatorInfrastructure.ActivitySourceName, ObservatorInfrastructure.Version);
using var activity = acs.StartActivity("TestApp.Main", ActivityKind.Internal);

// See https://aka.ms/new-console-template for more information
Console.WriteLine("=== Observator Test App ===");
Console.WriteLine($"ActivitySource Name: {ObservatorInfrastructure.ActivitySourceName}");
Console.WriteLine($"ActivitySource Version: {ObservatorInfrastructure.Version}");

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

var c = new Class1();
c.Foo();
// --- Interface Tracing Test Cases ---
Console.WriteLine("\n--- Testing Interface Tracing ---");
TestApp.IMyService myService = new TestApp.MyService();
string result1 = myService.DoSomething("hello interface");
Console.WriteLine($"DoSomething result: {result1}");

int result2 = myService.Calculate(10, 20);
Console.WriteLine($"Calculate result: {result2}");

// To verify tracing, check for activity/log output or inspect Activity.Current if available.