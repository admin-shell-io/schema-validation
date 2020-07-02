using ArgumentException = System.ArgumentException;
using Environment = System.Environment;
using Console = System.Console;
using FileInfo = System.IO.FileInfo;

// We can not cherry-pick imports from System.CommandLine since InvokeAsync is a necessary extension.
using System.CommandLine;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace ValidateJson
{
    class Program
    {
        private static int Handle(FileInfo model)
        {
            bool failed = false;
            try
            {
                IGraph graph = new Graph();
                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(graph, model.FullName);
                Console.Error.WriteLine($"OK: {model.FullName}");
            }
            catch (RdfParseException parseEx)
            {
                //This indicates a parser error e.g unexpected character, premature end of input, invalid syntax etc.
                Console.Error.WriteLine($"FAIL: {model.FullName}");
                Console.Error.WriteLine(parseEx.Message);
                failed = true;
            }
            catch (RdfException rdfEx)
            {
                //This represents a RDF error e.g. illegal triple for the given syntax, undefined namespace
                Console.Error.WriteLine($"FAIL: {model.FullName}");
                Console.WriteLine(rdfEx.Message);
                failed = true;
            }

            return (failed) ? 1 : 0;
        }

        private static int MainWithCode(string[] args)
        {
            var rootCommand = new RootCommand(
                "Validates the RDF file by parsing it.")
            {
                new Option<FileInfo>(
                    new[] {"--model", "-m"},
                    "Path to the RDF model in turtle")
                {
                    Required = true,
                    Argument = new Argument<FileInfo>().ExistingOnly()
                }
            };

            rootCommand.Handler = System.CommandLine.Invocation.CommandHandler.Create(
                (FileInfo model) => Handle(model));

            int exitCode = rootCommand.InvokeAsync(args).Result;
            return exitCode;
        }

        public static void Main(string[] args)
        {
            int exitCode = MainWithCode(args);
            Environment.ExitCode = exitCode;
        }
    }
}