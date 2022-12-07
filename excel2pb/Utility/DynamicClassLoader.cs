using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.Emit;

namespace excel2pb
{
    static class DynamicClassLoader
    {
        static Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();

        public static Assembly Compile(string codeIdentifier, IEnumerable<string> codes)
        {
            Assembly ret = null;
            if (_loadedAssemblies.TryGetValue(codeIdentifier, out ret))
            {
                return ret;
            }

            List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
            foreach (var item in codes)
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(item));

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")),
                MetadataReference.CreateFromFile("Google.Protobuf.dll"),
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        throw new Exception(string.Format("Failed to compile code '{0}'! {1}: {2}", codeIdentifier, diagnostic.Id, diagnostic.GetMessage()));
                    }
                    throw new Exception("Unknown error while compiling code '" + codeIdentifier + "'!");
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ret = Assembly.Load(ms.ToArray());
                    _loadedAssemblies[codeIdentifier] = ret;

                    return ret;
                }
            }
        }

        public static Type GetExtractClass(string codeIdentifier,string className)
        {
            if (!string.IsNullOrEmpty(className) && _loadedAssemblies.TryGetValue(codeIdentifier, out Assembly assembly))
            {
                var ret = assembly.GetType(className);
                if (ret == null)
                {
                    throw new Exception("Fail to find class '" + className + "' in code '" + codeIdentifier + "'!");
                }
                return ret;
            }
            return null;
        }
    }
}

