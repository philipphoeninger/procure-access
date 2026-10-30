// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ProcureAccess.Tests.Architecture.Modules;

/// <summary>
/// The project references of the solution, read from the <c>.csproj</c> files. ARC-03 checks these
/// and not the built assemblies: the compiler drops a reference to a project whose types are not
/// used, so the assembly metadata of an empty module would show nothing (S2-06 R2).
/// </summary>
internal sealed partial class ProjectGraph
{
    private const string SolutionFile = "ProcureAccess.sln";

    private readonly IReadOnlyDictionary<string, IReadOnlySet<string>> _references;

    /// <param name="references">Project name → names of the projects it references directly.</param>
    public ProjectGraph(IReadOnlyDictionary<string, IReadOnlySet<string>> references) => _references = references;

    /// <summary>The project names of the solution, each with the folder of its project file.</summary>
    public IReadOnlyDictionary<string, string> Folders { get; private init; } = new Dictionary<string, string>();

    public IEnumerable<string> Projects => _references.Keys;

    /// <summary>Reads every project of <c>ProcureAccess.sln</c> and its <c>ProjectReference</c> items.</summary>
    public static ProjectGraph LoadSolution()
    {
        var root = FindRepositoryRoot();
        var references = new Dictionary<string, IReadOnlySet<string>>();
        var folders = new Dictionary<string, string>();

        foreach (Match project in SolutionProjectLine().Matches(File.ReadAllText(Path.Combine(root, SolutionFile))))
        {
            var name = project.Groups["name"].Value;
            var relativePath = project.Groups["path"].Value.Replace('\\', '/');
            var document = XDocument.Load(Path.Combine(root, relativePath));

            references.Add(name, document.Descendants()
                .Where(element => element.Name.LocalName == "ProjectReference")
                .Select(element => ProjectName(element.Attribute("Include")?.Value
                    ?? throw new InvalidOperationException($"{relativePath}: ProjectReference without Include.")))
                .ToHashSet());
            folders.Add(name, Path.GetDirectoryName(relativePath)!.Replace('\\', '/'));
        }

        return new ProjectGraph(references) { Folders = folders };
    }

    public IReadOnlySet<string> DirectReferences(string project) =>
        _references.TryGetValue(project, out var references)
            ? references
            : throw new InvalidOperationException($"Project '{project}' is not in {SolutionFile}.");

    /// <summary>Direct references and everything they reference in turn (transitive references, S2-06 R3).</summary>
    public IReadOnlySet<string> AllReferences(string project)
    {
        var found = new HashSet<string>();
        var pending = new Stack<string>(DirectReferences(project));
        while (pending.TryPop(out var next))
        {
            if (found.Add(next) && _references.TryGetValue(next, out var further))
            {
                foreach (var reference in further)
                {
                    pending.Push(reference);
                }
            }
        }

        return found;
    }

    /// <summary>The projects that reference <paramref name="project"/> directly.</summary>
    public IEnumerable<string> ReferencedBy(string project) =>
        _references.Where(entry => entry.Value.Contains(project)).Select(entry => entry.Key);

    private static string ProjectName(string include) =>
        Path.GetFileNameWithoutExtension(include.Replace('\\', '/'));

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFile)))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException($"No {SolutionFile} above {AppContext.BaseDirectory}.");
    }

    // Project("{type}") = "Name", "path\to\Name.csproj", "{guid}"
    [GeneratedRegex("""^Project\("\{[^}]+\}"\) = "(?<name>[^"]+)", "(?<path>[^"]+\.csproj)", """, RegexOptions.Multiline)]
    private static partial Regex SolutionProjectLine();
}
