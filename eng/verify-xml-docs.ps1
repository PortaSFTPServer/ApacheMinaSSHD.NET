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
    Write-Error "Wrapper assembly was not found. Build ApacheMinaSSHD.NET.Wrapper before running the XML docs guard."
}

$AssemblyPath = $ResolvedAssemblyPath.Path

$XmlPath = [System.IO.Path]::ChangeExtension($AssemblyPath, ".xml")
if (!(Test-Path -LiteralPath $XmlPath)) {
    Write-Error "XML documentation file was not found: $XmlPath"
}

$GuardRoot = Join-Path ([System.IO.Path]::GetTempPath()) "ApacheMinaSSHD.NET-xml-docs-guard"
if (Test-Path -LiteralPath $GuardRoot) {
    Remove-Item -LiteralPath $GuardRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $GuardRoot | Out-Null

$ProjectPath = Join-Path $GuardRoot "XmlDocsGuard.csproj"
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
using System.Xml.Linq;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: XmlDocsGuard <assembly-path> <xml-doc-path>");
    return 2;
}

string assemblyPath = Path.GetFullPath(args[0]);
string xmlPath = Path.GetFullPath(args[1]);
string assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;

AssemblyLoadContext.Default.Resolving += (context, name) =>
{
    string candidate = Path.Combine(assemblyDirectory, name.Name + ".dll");
    return File.Exists(candidate)
        ? context.LoadFromAssemblyPath(candidate)
        : null;
};

HashSet<string> documentedMembers = XDocument.Load(xmlPath)
    .Descendants("member")
    .Select(member => (string?)member.Attribute("name"))
    .Where(name => !string.IsNullOrWhiteSpace(name))
    .Select(name => name!)
    .ToHashSet(StringComparer.Ordinal);

Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
SortedSet<string> missing = new(StringComparer.Ordinal);

foreach (Type type in assembly.GetExportedTypes())
{
    if (type.FullName?.StartsWith("ApacheMinaSSHD.NET.Wrapper.Internals.", StringComparison.Ordinal) == true)
    {
        continue;
    }

    RequireDocumented(GetTypeXmlName(type));

    BindingFlags flags = BindingFlags.Public
        | BindingFlags.Instance
        | BindingFlags.Static
        | BindingFlags.DeclaredOnly;

    foreach (ConstructorInfo constructor in type.GetConstructors(flags))
    {
        RequireDocumented(GetMemberXmlBaseName(constructor));
    }

    foreach (MethodInfo method in type.GetMethods(flags))
    {
        if (!method.IsSpecialName)
        {
            RequireDocumented(GetMemberXmlBaseName(method));
        }
    }

    foreach (PropertyInfo property in type.GetProperties(flags))
    {
        RequireDocumented(GetMemberXmlBaseName(property));
    }

    foreach (FieldInfo field in type.GetFields(flags))
    {
        if (!field.IsSpecialName)
        {
            RequireDocumented(GetMemberXmlBaseName(field));
        }
    }

    foreach (EventInfo eventInfo in type.GetEvents(flags))
    {
        RequireDocumented(GetMemberXmlBaseName(eventInfo));
    }
}

if (missing.Count > 0)
{
    Console.Error.WriteLine("Public API members missing XML documentation:");
    foreach (string member in missing)
    {
        Console.Error.WriteLine(" - " + member);
    }

    return 1;
}

Console.WriteLine("XML documentation guard passed.");
return 0;

void RequireDocumented(string xmlNameBase)
{
    if (documentedMembers.Contains(xmlNameBase))
    {
        return;
    }

    foreach (string documentedMember in documentedMembers)
    {
        if (documentedMember.StartsWith(xmlNameBase + "(", StringComparison.Ordinal))
        {
            return;
        }
    }

    missing.Add(xmlNameBase);
}

static string GetTypeXmlName(Type type) => "T:" + GetTypeName(type);

static string GetMemberXmlBaseName(MemberInfo member)
{
    if (member is ConstructorInfo constructor)
    {
        return "M:" + GetTypeName(constructor.DeclaringType!) + ".#ctor";
    }

    string prefix = member.MemberType switch
    {
        MemberTypes.Method => "M:",
        MemberTypes.Property => "P:",
        MemberTypes.Field => "F:",
        MemberTypes.Event => "E:",
        _ => throw new InvalidOperationException($"Unsupported member type: {member.MemberType}")
    };

    return prefix + GetTypeName(member.DeclaringType!) + "." + member.Name;
}

static string GetTypeName(Type type)
{
    string name = type.FullName ?? type.Name;
    int tickIndex = name.IndexOf('`');
    name = tickIndex >= 0
        ? name[..tickIndex]
        : name;

    return name.Replace('+', '.');
}
'@ | Set-Content -LiteralPath $ProgramPath -Encoding UTF8

& dotnet run --project $ProjectPath -c Release -- $AssemblyPath $XmlPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
