using ArgumentException = System.ArgumentException;
using Environment = System.Environment;
using Console = System.Console;
using FileInfo = System.IO.FileInfo;
using System.Collections.Generic;
using Path = System.IO.Path;
using System.Linq;

// We can not cherry-pick imports from System.CommandLine since InvokeAsync is a necessary extension.
using System.CommandLine;
using System.Xml;
using System.Xml.Schema;

namespace ValidateJson
{
    class Program
    {
        private static int Handle(string[] inputs, FileInfo schema)
        {
            // Load the schema

            XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
            xmlSchemaSet.XmlResolver = new XmlUrlResolver();

            xmlSchemaSet.Add(null, schema.FullName);

            var schemaMessages = new List<string>();
            xmlSchemaSet.ValidationEventHandler += (object sender, ValidationEventArgs e) =>
            {
                schemaMessages.Add(e.Message);
            };
            xmlSchemaSet.Compile();

            if (schemaMessages.Count > 0)
            {
                Console.Error.WriteLine($"Failed to compile the schema: {schema}");
                foreach (string message in schemaMessages)
                {
                    Console.Error.WriteLine(message);
                    return 1;
                }
            }

            // Validate

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
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.ValidationType = ValidationType.Schema;
                    settings.Schemas = xmlSchemaSet;

                    var messages = new List<string>();
                    settings.ValidationEventHandler += (object sender, ValidationEventArgs e) =>
                    {
                        messages.Add(e.Message);
                    };

                    XmlReader reader = XmlReader.Create(path, settings);

                    while (reader.Read())
                    {
                        // Invoke callbacks
                    };

                    if (messages.Count > 0)
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
                "Validates the XML files given the XSD schema.")
            {
                new Option<string[]>(
                        new[] {"--inputs", "-i"},
                        "Glob patterns of the files to be validated")
                    {Required = true},

                new Option<FileInfo>(
                    new[] {"--schema", "-s"},
                    "Path to the XSD schema")
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