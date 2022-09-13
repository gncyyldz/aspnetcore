// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Microsoft.AspNetCore.Analyzers.RouteEmbeddedLanguage;

public static class CSharpRouteParameterUnusedParameterCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : RoutePatternAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId = null)
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, XUnitVerifier>.Diagnostic(diagnosticId);

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => new DiagnosticResult(descriptor);

    public static Task VerifyCodeFixAsync(string source, string fixedSource)
        => VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

    public static Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
        => VerifyCodeFixAsync(source, new[] { expected }, fixedSource);

    public static Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource)
        => VerifyCodeFixAsync(source, expected, fixedSource, string.Empty);

    public static Task VerifyCodeFixAsync(string sources, DiagnosticResult[] expected, string fixedSources, string usageSource = "", int? expectedIterations = null)
    {
        var test = new RoutePatternAnalyzerTest
        {
            TestState =
            {
                Sources = { sources, usageSource },
                // We need to set the output type to an exe to properly
                // support top-level programs in the tests. Otherwise,
                // the test infra will assume we are trying to build a library.
                OutputKind = OutputKind.ConsoleApplication
            },
            FixedState =
            {
                Sources =  { fixedSources, usageSource }
            }
        };

        test.NumberOfFixAllIterations = expectedIterations;
        test.TestState.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }

    public class RoutePatternAnalyzerTest : CSharpCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
    {
        public RoutePatternAnalyzerTest()
        {
            // We populate the ReferenceAssemblies used in the tests with the locally-built AspNetCore
            // assemblies that are referenced in a minimal app to ensure that there are no reference
            // errors during the build. The value used here should be updated on each TFM change.
            ReferenceAssemblies = ReferenceAssemblies.Net.Net70.AddAssemblies(ImmutableArray.Create(
                TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Builder.WebApplication).Assembly.Location),
                TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions).Assembly.Location),
                TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Builder.IApplicationBuilder).Assembly.Location),
                TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Builder.IEndpointConventionBuilder).Assembly.Location),
                TrimAssemblyExtension(typeof(Microsoft.Extensions.Hosting.IHost).Assembly.Location),
                TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Mvc.ModelBinding.IBinderTypeProviderMetadata).Assembly.Location),
                TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Mvc.BindAttribute).Assembly.Location),
                TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Http.IFormFileCollection).Assembly.Location),
                TrimAssemblyExtension(typeof(System.IO.Pipelines.PipeReader).Assembly.Location)));

            string TrimAssemblyExtension(string fullPath) => fullPath.Replace(".dll", string.Empty);
        }
    }
}
