using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities.Collections;
using static Serilog.Log;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	Target UpdateReadme => _ => _
		.DependsOn(Clean)
		.Before(Compile)
		.Executes(() =>
		{
			string version = string.Join('.', GitVersion.SemVer.Split('.').Take(3));
			if (version.IndexOf('-') != -1)
			{
				version = version.Substring(0, version.IndexOf('-'));
			}

			StringBuilder sb = new();
			string[] lines = File.ReadAllLines(Solution.Directory / "README.md");
			sb.AppendLine(lines.First());
			sb.AppendLine(
				$"[![Changelog](https://img.shields.io/badge/Changelog-v{version}-blue)](https://github.com/aweXpect/aweXpect.Web/releases/tag/v{version})");
			foreach (string line in lines.Skip(1))
			{
				if (line.StartsWith("[![Build](https://github.com/aweXpect/aweXpect.Web/actions/workflows/build.yml") ||
				    line.StartsWith("[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure"))
				{
					continue;
				}

				if (line.StartsWith("[![Coverage](https://sonarcloud.io/api/project_badges/measure"))
				{
					sb.AppendLine(line
						.Replace(")", $"&branch=release/v{version})"));
					continue;
				}

				if (line.StartsWith("[![Mutation testing badge](https://img.shields.io/endpoint"))
				{
					sb.AppendLine(line
						.Replace("%2Fmain)", $"%2Frelease%2Fv{version})")
						.Replace("/main)", $"/release/v{version})"));
					continue;
				}

				sb.AppendLine(line);
			}

			File.WriteAllText(ArtifactsDirectory / "README.md", sb.ToString());
		});

	Target Pack => _ => _
		.DependsOn(UpdateReadme)
		.DependsOn(Compile)
		.Executes(() =>
		{
			AbsolutePath packagesDirectory = ArtifactsDirectory / "Packages";
			packagesDirectory.CreateOrCleanDirectory();

			List<string> packages = new();
			foreach (Project project in new[]
			         {
				         Solution.aweXpect_Web,
			         })
			{
				foreach (string package in
				         Directory.EnumerateFiles(project.Directory / "bin", "*.nupkg", SearchOption.AllDirectories))
				{
					File.Move(package, packagesDirectory / Path.GetFileName(package));
					Debug("Found nuget package: {PackagePath}", package);
					packages.Add(Path.GetFileName(package));
				}

				foreach (string symbolPackage in
				         Directory.EnumerateFiles(project.Directory / "bin", "*.snupkg", SearchOption.AllDirectories))
				{
					File.Move(symbolPackage, packagesDirectory / Path.GetFileName(symbolPackage));
					Debug("Found symbol package: {PackagePath}", symbolPackage);
				}
			}

			ReportSummary(s => s
				.AddPair("Packages", string.Join(", ", packages)));
		});
}
