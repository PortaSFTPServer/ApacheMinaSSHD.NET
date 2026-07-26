[CmdletBinding()]
param(
    [string]$Configuration = "Debug",
    [string]$TargetFramework = "net10.0",
    [string]$AssemblyPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path

if ([string]::IsNullOrWhiteSpace($AssemblyPath)) {
    $AssemblyPath = Join-Path $RepoRoot "ApacheMinaSSHD.NET.Wrapper/bin/$Configuration/$TargetFramework/ApacheMinaSSHD.NET.Wrapper.dll"
}

$ResolvedAssemblyPath = Resolve-Path -LiteralPath $AssemblyPath -ErrorAction SilentlyContinue
if ($null -eq $ResolvedAssemblyPath -or [string]::IsNullOrWhiteSpace($ResolvedAssemblyPath.Path)) {
    Write-Error "Wrapper assembly was not found. Build ApacheMinaSSHD.NET.Wrapper before running the public API guard."
}

$AssemblyPath = $ResolvedAssemblyPath.Path

$GuardRoot = Join-Path ([System.IO.Path]::GetTempPath()) "ApacheMinaSSHD.NET-api-guard"
if (Test-Path -LiteralPath $GuardRoot) {
    Remove-Item -LiteralPath $GuardRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $GuardRoot | Out-Null

$ProjectPath = Join-Path $GuardRoot "ApiGuard.csproj"
$ProgramPath = Join-Path $GuardRoot "Program.cs"

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>$TargetFramework</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
"@ | Set-Content -LiteralPath $ProjectPath -Encoding UTF8

@'
using System.Reflection;
using System.Runtime.Loader;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: ApiGuard <assembly-path>");
    return 2;
}

string assemblyPath = Path.GetFullPath(args[0]);
string assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;

AssemblyLoadContext.Default.Resolving += (context, name) =>
{
    string candidate = Path.Combine(assemblyDirectory, name.Name + ".dll");
    if (File.Exists(candidate))
    {
        return context.LoadFromAssemblyPath(candidate);
    }

    string? nugetGlobalPackages = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
    string ikvmLib = Path.Combine(nugetGlobalPackages, "ikvm", "8.15.0", "ref", "net8.0", name.Name + ".dll");
    if (File.Exists(ikvmLib))
    {
        return context.LoadFromAssemblyPath(ikvmLib);
    }

    return null;
};

Assembly assembly;
Type[] exportedTypes;
try
{
    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
    exportedTypes = assembly.GetExportedTypes();
}
catch (ReflectionTypeLoadException ex)
{
    Console.Error.WriteLine("Unable to load all public API types.");
    foreach (Exception? loaderException in ex.LoaderExceptions)
    {
        if (loaderException is not null)
        {
            Console.Error.WriteLine(loaderException.Message);
        }
    }

    return 3;
}

string[] blockedPrefixes =
[
    "java.",
    "javax.",
    "org.apache.",
    "org.slf4j.",
    "ikvm."
];

string[] allowedLeakLocations =
[
    "AMNetSshServer.getAttribute",
    "AMNetSshServer.setAttribute",
    "AMNetSshServer.getIoServiceFactoryFactory",
    "AMNetSshServer.setIoServiceFactoryFactory",
    "AMNetSshServer.getScheduledExecutorService",
    "AMNetSshServer.setScheduledExecutorService",
    "AMNetSshServer.getServiceFactories",
    "AMNetSshServer.setServiceFactories",
    "AMNetSshServer.getUserAuthFactories",
    "AMNetSshServer.setUserAuthFactories"
];

SortedSet<string> leaks = new(StringComparer.Ordinal);

foreach (Type type in exportedTypes)
{
    CheckType($"type {FormatType(type)}", type);

    if (type.BaseType is not null)
    {
        CheckType($"{FormatType(type)} base type", type.BaseType);
    }

    foreach (Type interfaceType in type.GetInterfaces())
    {
        CheckType($"{FormatType(type)} interface", interfaceType);
    }

    BindingFlags flags = BindingFlags.Public
        | BindingFlags.Instance
        | BindingFlags.Static
        | BindingFlags.DeclaredOnly;

    foreach (ConstructorInfo constructor in type.GetConstructors(flags))
    {
        foreach (ParameterInfo parameter in constructor.GetParameters())
        {
            CheckType($"{FormatType(type)} constructor parameter {parameter.Name}", parameter.ParameterType);
        }
    }

    foreach (MethodInfo method in type.GetMethods(flags))
    {
        CheckType($"{FormatType(type)}.{method.Name} return type", method.ReturnType);
        foreach (ParameterInfo parameter in method.GetParameters())
        {
            CheckType($"{FormatType(type)}.{method.Name} parameter {parameter.Name}", parameter.ParameterType);
        }
    }

    foreach (PropertyInfo property in type.GetProperties(flags))
    {
        CheckType($"{FormatType(type)}.{property.Name} property type", property.PropertyType);
    }

    foreach (EventInfo eventInfo in type.GetEvents(flags))
    {
        CheckType($"{FormatType(type)}.{eventInfo.Name} event type", eventInfo.EventHandlerType);
    }

    foreach (FieldInfo field in type.GetFields(flags))
    {
        CheckType($"{FormatType(type)}.{field.Name} field type", field.FieldType);
    }
}

if (leaks.Count > 0)
{
    Console.Error.WriteLine("Public API exposes Java, Apache MINA, SLF4J, or IKVM types:");
    foreach (string leak in leaks)
    {
        Console.Error.WriteLine(" - " + leak);
    }

    return 1;
}

Console.WriteLine("Public API guard passed.");
return 0;

void CheckType(string location, Type? type)
{
    if (type is null)
    {
        return;
    }

    foreach (Type candidate in Flatten(type))
    {
        string fullName = candidate.FullName ?? candidate.Name;
        if (blockedPrefixes.Any(prefix => fullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            if (!allowedLeakLocations.Any(loc => location.Contains(loc, StringComparison.Ordinal)))
            {
                leaks.Add($"{location}: {FormatType(candidate)}");
            }
        }
    }
}

static IEnumerable<Type> Flatten(Type type)
{
    while (type.HasElementType)
    {
        type = type.GetElementType()!;
    }

    yield return type;

    if (type.IsGenericType)
    {
        yield return type.GetGenericTypeDefinition();
        foreach (Type genericArgument in type.GetGenericArguments())
        {
            foreach (Type nested in Flatten(genericArgument))
            {
                yield return nested;
            }
        }
    }
}

static string FormatType(Type type) => type.FullName ?? type.Name;
'@ | Set-Content -LiteralPath $ProgramPath -Encoding UTF8

& dotnet run --project $ProjectPath -c Release -- $AssemblyPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
