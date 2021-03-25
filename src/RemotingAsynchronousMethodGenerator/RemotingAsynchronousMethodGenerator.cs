using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RemotingAsynchronousMethodGenerator
{
    [Generator]
    internal partial class RemotingAsynchronousMethodGenerator : ISourceGenerator
    {
        private const string attributeText = @"
using System;

namespace RemotingAsynchronousMethodGenerator
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    [System.Diagnostics.Conditional(""RemotingAsynchronousMethodGenerator_DEBUG"")]
    public sealed class GenerateAsynchronousMethodsAttribute : Attribute
    {
    }
}
";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization((i) => i.AddSource("AsyncMethodAttribute", attributeText));
            context.RegisterForSyntaxNotifications(() => new SyncMethodSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is SyncMethodSyntaxReceiver receiver)
            {
                foreach (IGrouping<INamedTypeSymbol, IMethodSymbol> methodSymbols in receiver.Methods.GroupBy(f => f.ContainingType))
                {
                    using var writer = new StringWriter();
                    using var indentWriter = new IndentedTextWriter(writer);

                    INamedTypeSymbol namedTypeSymbol = methodSymbols.Key;

                    indentWriter.WriteLine("using System;");
                    indentWriter.WriteLine("using System.Threading.Tasks;");
                    indentWriter.WriteLine();

                    if (!namedTypeSymbol.ContainingNamespace.IsGlobalNamespace)
                    {
                        indentWriter.WriteLine($"namespace {namedTypeSymbol.ContainingNamespace.ToDisplayString()}");
                        indentWriter.WriteLine("{");
                        indentWriter.Indent++;
                    }

                    indentWriter.WriteLine($"public static partial class {namedTypeSymbol.Name}AsyncMethodExtensions");
                    indentWriter.WriteLine("{");
                    indentWriter.Indent++;

                    foreach (var methodSymbol in methodSymbols)
                    {
                        bool returnVoidType = methodSymbol.ReturnType.SpecialType == SpecialType.System_Void;
                        string returnTypeName = returnVoidType ? "bool" : methodSymbol.ReturnType.ToString();
                        indentWriter.Write($"public static ");
                        if (returnVoidType)
                        {
                            indentWriter.Write("Task ");
                        }
                        else
                        {
                            indentWriter.Write($"Task<{methodSymbol.ReturnType}> ");
                        }

                        indentWriter.Write($"{methodSymbol.Name}Async(this {namedTypeSymbol.Name} proxy");

                        if (methodSymbol.Parameters.Length == 0)
                        {
                            indentWriter.WriteLine(")");
                        }
                        else
                        {
                            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                            {
                                indentWriter.Write($", {methodSymbol.Parameters[i].Type} {methodSymbol.Parameters[i].Name}");
                            }

                            indentWriter.WriteLine(")");
                        }

                        indentWriter.WriteLine("{");
                        indentWriter.Indent++;

                        indentWriter.WriteLine("if (proxy == null)");
                        indentWriter.WriteLine("{");
                        indentWriter.Indent++;
                        indentWriter.WriteLine("throw new ArgumentNullException(nameof(proxy));");
                        indentWriter.Indent--;
                        indentWriter.WriteLine("}");

                        indentWriter.WriteLine();

                        indentWriter.WriteLine($"TaskCompletionSource<{returnTypeName}> tcs = new TaskCompletionSource<{returnTypeName}>();");
                        indentWriter.WriteLine();

                        if (returnVoidType)
                        {
                            indentWriter.Write($"Action<");

                            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                            {
                                if (i == methodSymbol.Parameters.Length - 1)
                                {
                                    indentWriter.Write(methodSymbol.Parameters[i].Type);
                                }
                                else
                                {
                                    indentWriter.Write($"{methodSymbol.Parameters[i].Type}, ");
                                }
                            }
                        }
                        else
                        {
                            indentWriter.Write($"Func<");

                            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                            {
                                indentWriter.Write($"{methodSymbol.Parameters[i].Type}, ");
                            }

                            indentWriter.Write($"{methodSymbol.ReturnType}");
                        }

                        indentWriter.WriteLine($"> @delegate = proxy.{methodSymbol.Name};");

                        indentWriter.Write("@delegate.BeginInvoke(");
                        for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                        {
                            indentWriter.Write($"{methodSymbol.Parameters[i].Name}, ");
                        }

                        indentWriter.WriteLine("iar =>");
                        indentWriter.WriteLine("{");
                        indentWriter.Indent++;

                        indentWriter.WriteLine($"TaskCompletionSource<{returnTypeName}> taskCompletionSource = iar.AsyncState as TaskCompletionSource<{returnTypeName}>;");
                        indentWriter.WriteLine();

                        indentWriter.WriteLine("try");
                        indentWriter.WriteLine("{");
                        indentWriter.Indent++;
                        indentWriter.WriteLine($"{(returnVoidType ? string.Empty : $"{returnTypeName} result = ")}@delegate.EndInvoke(iar);");
                        indentWriter.WriteLine($"taskCompletionSource.TrySetResult({(returnVoidType ? "true" : "result")});");
                        indentWriter.Indent--;
                        indentWriter.WriteLine("}");
                        indentWriter.WriteLine("catch (OperationCanceledException)");
                        indentWriter.WriteLine("{");
                        indentWriter.Indent++;
                        indentWriter.WriteLine("taskCompletionSource.TrySetCanceled();");
                        indentWriter.Indent--;
                        indentWriter.WriteLine("}");
                        indentWriter.WriteLine("catch (Exception ex)");
                        indentWriter.WriteLine("{");
                        indentWriter.Indent++;
                        indentWriter.WriteLine("taskCompletionSource.TrySetException(ex);");
                        indentWriter.Indent--;
                        indentWriter.WriteLine("}");

                        indentWriter.Indent--;
                        indentWriter.WriteLine("}, tcs);");

                        indentWriter.WriteLine();
                        indentWriter.WriteLine("return tcs.Task;");

                        indentWriter.Indent--;
                        indentWriter.WriteLine("}");
                        indentWriter.WriteLine();
                    }

                    indentWriter.Indent--;
                    indentWriter.WriteLine("}");

                    if (!namedTypeSymbol.ContainingNamespace.IsGlobalNamespace)
                    {
                        indentWriter.Indent--;
                        indentWriter.WriteLine("}");
                    }

                    //context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(string.Empty, string.Empty, writer.ToString().Replace(Environment.NewLine, string.Empty), string.Empty, DiagnosticSeverity.Info, true), Location.None));
                    context.AddSource($"{namedTypeSymbol.Name}AsyncMethodExtensions", SourceText.From(writer.ToString(), Encoding.UTF8));
                }
            }
        }
    }
}
