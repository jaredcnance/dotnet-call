﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace DotnetCall
{
    class Program
    {
        static Program()
        {
            var envLogValue = Environment.GetEnvironmentVariable("DOTNET_CALL_LOG");
            _log = bool.TryParse(envLogValue, out bool shouldLog) ? shouldLog : false;
        }

        private static bool _log;
        private static void Log(string message, bool forceLog = false, params string[] format)
        {
            if (_log || forceLog)
                Console.WriteLine(message);
        }

        // dotnet call -a assembly -c namespace.class -m method        
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);
            var assemblyOption = commandLineApplication.Option("-a |--assembly <assembly>", "The assembly name", CommandOptionType.SingleValue);
            var classOption = commandLineApplication.Option("-c |--class <class>", "The fully qualified class name", CommandOptionType.SingleValue);
            var methodOption = commandLineApplication.Option("-m |--method <method>", "The public method name", CommandOptionType.SingleValue);
            var dataOption = commandLineApplication.Option("-d |--data <data>", "JSON input data", CommandOptionType.SingleValue);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                if (assemblyOption.HasValue() && classOption.HasValue() && methodOption.HasValue())
                {
                    var options = new CliOptions(assemblyOption.Value(), classOption.Value(), methodOption.Value(), dataOption.Value());
                    Invoke(options);
                    return 0;
                }
                
                Log("Required input was not supplied", forceLog: true);
                return 1;
            });

            commandLineApplication.Execute(args);
        }

        static void Invoke(CliOptions options)
        {
            var directory = Directory.GetCurrentDirectory();
            var assemblyPath = FindFile(directory, options.AssemblyName);
            Log($"Loading assembly {assemblyPath}");

            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            var myType = myAssembly.GetType($"{options.FullyQualifiedClassName}");
            if(myType == null)
            {
                try 
                {
                    Console.WriteLine($"Type '{options.FullyQualifiedClassName}' not found in assembly '{myAssembly}'. The following types are available:");
                    var types = myAssembly.GetTypes();
                    foreach(var definedType in types)
                    Console.WriteLine($" - {definedType.Name}");
                }
                catch(Exception e)
                {
                     Console.WriteLine(JsonConvert.SerializeObject(e));
                     throw;
                }
            }

            var myInstance = Activator.CreateInstance(myType);
            var method = myType.GetTypeInfo().GetMethod(options.MethodName);

            var parameters = method.GetParameters();
            if (parameters.Length > 1) throw new Exception("dotnet exec does not currently support invocations with multiple parameters");

            var param = parameters[0];
            Log($"Method {method.Name} found with {parameters.Length} params with type {param.ParameterType}");

            Log($"Deserializing {options.Data} to {param.ParameterType}");

            var instance = JObject.Parse(options.Data).ToObject(param.ParameterType);

            method.Invoke(myInstance, new[] { instance });
        }

        static string FindFile(string directory, string assembly, bool throwIfNotFound = true)
        {
            Log($"Looking for {assembly} in {directory}");
            foreach (string d in Directory.GetDirectories(directory))
            {
                var files = Directory.GetFiles(d, $"{assembly}.dll");
                
                Log($"Found {files.Length} files");

                if(files.Length == 1)
                    return files[0];
                else if(files.Length > 1)
                    throw new Exception("Found more than one matching assembly");

                var subdirResult = FindFile(d, assembly, throwIfNotFound: false);
                if(subdirResult != null)
                    return subdirResult;
            }

            if(throwIfNotFound)
                throw new Exception("Could not find the assembly");

            return null;
        }
    }
}
