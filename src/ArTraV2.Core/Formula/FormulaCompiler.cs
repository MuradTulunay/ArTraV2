using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ArTraV2.Core.Formula;

public class CompilerError
{
    public string Message { get; set; } = "";
    public int Line { get; set; }
    public int Column { get; set; }
    public string Id { get; set; } = "";
    public bool IsWarning { get; set; }
}

public class CompileResult
{
    public Assembly? Assembly { get; set; }
    public List<CompilerError> Errors { get; } = [];
    public bool Success => Assembly != null && !Errors.Any(e => !e.IsWarning);
    public double CompileTimeMs { get; set; }
}

public static class FormulaCompiler
{
    /// <summary>
    /// Compile C# formula source code into an in-memory assembly.
    /// The source should contain classes inheriting from FormulaBase in namespace FML.
    /// </summary>
    public static CompileResult Compile(string sourceCode, string? assemblyName = null)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = new CompileResult();

        assemblyName ??= $"FML_{Guid.NewGuid():N}";

        // Wrap source if it doesn't have using statements
        var fullSource = EnsureUsings(sourceCode);

        var syntaxTree = CSharpSyntaxTree.ParseText(fullSource);

        // Reference assemblies
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(FormulaBase).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
        };

        // Add runtime assemblies
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var runtimeAssembly = Path.Combine(runtimeDir, "System.Runtime.dll");
        if (File.Exists(runtimeAssembly))
            references.Add(MetadataReference.CreateFromFile(runtimeAssembly));
        var collectionsAssembly = Path.Combine(runtimeDir, "System.Collections.dll");
        if (File.Exists(collectionsAssembly))
            references.Add(MetadataReference.CreateFromFile(collectionsAssembly));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);

        sw.Stop();
        result.CompileTimeMs = sw.Elapsed.TotalMilliseconds;

        if (!emitResult.Success)
        {
            foreach (var diag in emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning))
            {
                var lineSpan = diag.Location.GetLineSpan();
                result.Errors.Add(new CompilerError
                {
                    Message = diag.GetMessage(),
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    Id = diag.Id,
                    IsWarning = diag.Severity == DiagnosticSeverity.Warning
                });
            }
            return result;
        }

        ms.Seek(0, SeekOrigin.Begin);
        result.Assembly = Assembly.Load(ms.ToArray());
        return result;
    }

    /// <summary>
    /// Compile and register the assembly with FormulaBase.
    /// Returns compile result.
    /// </summary>
    public static CompileResult CompileAndRegister(string sourceCode, string key)
    {
        var result = Compile(sourceCode, key);
        if (result.Success && result.Assembly != null)
        {
            FormulaBase.RegAssembly(key, result.Assembly);
        }
        return result;
    }

    /// <summary>
    /// Compile a single formula class from simplified code.
    /// Wraps in namespace FML and class structure if needed.
    /// </summary>
    public static CompileResult CompileFormula(string name, string code, List<FormulaParam>? parameters = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using ArTraV2.Core.Formula;");
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("namespace FML");
        sb.AppendLine("{");
        sb.AppendLine($"    [Serializable]");
        sb.AppendLine($"    public class {name} : FormulaBase");
        sb.AppendLine("    {");

        // Parameter fields
        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                if (p.ParamType == FormulaParamType.Double)
                    sb.AppendLine($"        public double {p.Name} = {p.DefaultValue};");
                else
                    sb.AppendLine($"        public string {p.Name} = \"{p.DefaultValue}\";");
            }
            sb.AppendLine();

            // Constructor
            sb.AppendLine($"        public {name}() : base()");
            sb.AppendLine("        {");
            foreach (var p in parameters)
            {
                sb.AppendLine($"            AddParam(\"{p.Name}\",\"{p.DefaultValue}\",\"{p.MinValue}\",\"{p.MaxValue}\",\"{p.Step}\",\"{p.Description}\",FormulaParamType.{p.ParamType});");
            }
            sb.AppendLine("        }");
        }
        else
        {
            sb.AppendLine($"        public {name}() : base() {{ }}");
        }

        sb.AppendLine();
        sb.AppendLine("        public override FormulaPackage Run(IFormulaDataProvider DP)");
        sb.AppendLine("        {");
        sb.AppendLine("            this.DataProvider = DP;");
        sb.AppendLine($"            {code}");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return Compile(sb.ToString(), $"FML_{name}");
    }

    private static string EnsureUsings(string source)
    {
        if (source.Contains("using ArTraV2.Core.Formula;")) return source;
        return $"using ArTraV2.Core.Formula;\nusing System;\nusing System.Linq;\n\n{source}";
    }

    /// <summary>
    /// Get template source code for a new formula
    /// </summary>
    public static string GetTemplate(string name) => $@"using ArTraV2.Core.Formula;
using System;

namespace FML
{{
    [Serializable]
    public class {name} : FormulaBase
    {{
        public double N1 = 14;

        public {name}() : base()
        {{
            AddParam(""N1"",""14"",""1"",""300"",""1"","""",FormulaParamType.Double);
        }}

        public override FormulaPackage Run(IFormulaDataProvider DP)
        {{
            this.DataProvider = DP;
            FormulaData result = MA(CLOSE, N1);
            result.Name = ""Result"";
            return new FormulaPackage(new FormulaData[]{{ result }}, """");
        }}

        public override string LongName {{ get {{ return ""{name}""; }} }}
        public override string Description {{ get {{ return ""Custom indicator""; }} }}
    }}
}}";
}
