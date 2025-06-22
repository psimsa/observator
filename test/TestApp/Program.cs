using System;
using Observator.Generated;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("=== Observator Test App ===");
Console.WriteLine($"ActivitySource Name: {ObservatorInfrastructure.ActivitySourceName}");
Console.WriteLine($"ActivitySource Version: {ObservatorInfrastructure.Version}");
Console.WriteLine($"ActivitySource Instance: {ObservatorInfrastructure.ActivitySource}");
Console.WriteLine($"Meter Instance: {ObservatorInfrastructure.Meter}");

var sampleService = new TestApp.SampleService();
var greetResult = sampleService.Greet("World");
Console.WriteLine($"Greet result: {greetResult}");

Console.WriteLine("Test completed successfully!");
