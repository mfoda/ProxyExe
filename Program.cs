using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace ProxyExe
{
    class Program
    {
        static string source = @"
            using System;
            using System.IO;
            using System.Diagnostics;
            class Program
            {
                static int Main(string[] args)
                {
                 using (Process process = new Process())
                 {
                     process.StartInfo.FileName               = $fileName;
                     process.StartInfo.Arguments              = string.Join("" "", args);
                     process.StartInfo.WorkingDirectory       = $workingDir;
                     process.StartInfo.UseShellExecute        = false;
                     process.StartInfo.RedirectStandardOutput = true;
                     process.Start();
                     process.WaitForExit();
                     return process.ExitCode;
                  }
                }

            }";

        static void Main(string[] args)
        {
            
            var target     = @"\\?\" + Path.GetFullPath(args[0]);
            var fileName   = Path.GetFileName(target);
            var workingDir = Path.GetDirectoryName(target);

            source = source.Replace("$fileName", $"@\"{fileName}\"");
            source = source.Replace("$workingDir", $"@\"{workingDir}\"");

            var provider   = new CSharpCodeProvider();
            var parameters = new CompilerParameters()
            {
                GenerateInMemory   = true,
                GenerateExecutable = true,
                OutputAssembly     = Path.GetFullPath(args[1])
            };

            parameters.ReferencedAssemblies.AddRange( new string[] { "mscorlib.dll", "System.Core.dll", "System.dll" } );
            var result = provider.CompileAssemblyFromSource(parameters, source);

            if (result.Errors.Count == 0)
                Console.WriteLine("Source {0} built into {1} successfully.", args[0], result.PathToAssembly);
            else
            {
                Console.WriteLine("Errors building {0} into {1}", args[0], result.PathToAssembly);
                foreach (CompilerError error in result.Errors)
                {
                    Console.WriteLine("  {0}", error.ToString());
                    Console.WriteLine();
                }
            }
        }
    }
}
