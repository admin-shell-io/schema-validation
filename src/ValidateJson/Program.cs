using ArgumentException = System.ArgumentException;
using Environment = System.Environment;
using Console = System.Console;
using FileInfo = System.IO.FileInfo;
using System.Collections.Generic;
using Path = System.IO.Path;
using System.Linq;

// We can not cherry-pick imports from Newtonsoft.Json.Schema since we need the extension IsValid. 
using Newtonsoft.Json.Schema;

// We can not cherry-pick imports from System.CommandLine since InvokeAsync is a necessary extension.
using System.CommandLine;

namespace ValidateJson
{
    class Program
    {
        private static int Handle(string[] inputs, FileInfo schema)
        {
            string schemaText = System.IO.File.ReadAllText(schema.FullName);
            var jSchema = Newtonsoft.Json.Schema.JSchema.Parse(schemaText);

            string cwd = System.IO.Directory.GetCurrentDirectory();

            bool failed = false;

            foreach (string pattern in inputs)
            {
                IEnumerable<string> paths;
                if (Path.IsPathRooted(pattern))
                {
                    var root = Path.GetPathRoot(pattern);
                    if (root == null)
                    {
                        throw new ArgumentException(
                            $"Root could not be retrieved from rooted pattern: {pattern}");
                    }

                    var relPattern = Path.GetRelativePath(root, pattern);
                    paths = GlobExpressions.Glob.Files(root, relPattern)
                        .Select((path) => Path.Join(root, relPattern));
                }
                else
                {
                    paths = GlobExpressions.Glob.Files(cwd, pattern)
                        .Select((path) => Path.Join(cwd, path));
                }

                foreach (string path in paths)
                {
                    string text = System.IO.File.ReadAllText(path);
                    var jObject = Newtonsoft.Json.Linq.JObject.Parse(text);

                    bool valid = jObject.IsValid(jSchema, out IList<string> messages);

                    if (!valid)
                    {
                        Console.Error.WriteLine($"FAIL: {path}");
                        foreach (string message in messages)
                        {
                            Console.Error.WriteLine(message);
                        }

                        failed = true;
                    }
                    else
                    {
                        Console.WriteLine($"OK: {path}");
                    }


                }
            }

            return (failed) ? 1 : 0;
        }

        private static int MainWithCode(string[] args)
        {
            var rootCommand = new RootCommand(
                "Validates the JSON files given the JSON Schema.")
            {
                new Option<string[]>(
                        new[] {"--inputs", "-i"},
                        "Glob patterns of the files to be validated")
                    {Required = true},

                new Option<FileInfo>(
                    new[] {"--schema", "-s"},
                    "Path to the JSON schema")
                {
                    Required = true,
                    Argument = new Argument<FileInfo>().ExistingOnly()
                }
            };

            rootCommand.Handler = System.CommandLine.Invocation.CommandHandler.Create(
                (string[] inputs, FileInfo schema) => Handle(inputs, schema));

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