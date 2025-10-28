using System;
using System.Text.Json;

var type = typeof(JsonNamingPolicy);
var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

Console.WriteLine("Available JsonNamingPolicy static properties:");
foreach (var prop in properties)
{
    Console.WriteLine($"  - {prop.Name}");
}
