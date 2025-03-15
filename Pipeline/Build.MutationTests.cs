using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Octokit;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using ProductHeaderValue = Octokit.ProductHeaderValue;
using Project = Nuke.Common.ProjectModel.Project;

// ReSharper disable UnusedMember.Local
// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	static string MutationCommentBody = "";

	Target MutationTests => _ => _
		.DependsOn(MutationTestExecution)
		.DependsOn(MutationComment);

	Target MutationTestExecution => _ => _
		.DependsOn(Compile)
		.Executes(() =>
		{
			AbsolutePath toolPath = TestResultsDirectory / "dotnet-stryker";
			AbsolutePath configFile = toolPath / "Stryker.Config.json";
			AbsolutePath strykerOutputDirectory = ArtifactsDirectory / "Stryker";
			strykerOutputDirectory.CreateOrCleanDirectory();
			toolPath.CreateOrCleanDirectory();

			DotNetToolInstall(_ => _
				.SetPackageName("dotnet-stryker")
				.SetToolInstallationPath(toolPath));

			Dictionary<Project, Project[]> projects = new()
			{
				{
					Solution.aweXpect_Web, [
						Solution.Tests.aweXpect_Web_Tests,
						Solution.Tests.aweXpect_Web_Internal_Tests,
						Solution.Tests.aweXpect_Web_Samples_Tests,
					]
				},
			};

			foreach (KeyValuePair<Project, Project[]> project in projects)
			{
				string branchName = BranchName;
				if (GitHubActions?.Ref.StartsWith("refs/tags/", StringComparison.OrdinalIgnoreCase) == true)
				{
					string version = GitHubActions.Ref.Substring("refs/tags/".Length);
					branchName = "release/" + version;
					Log.Information("Use release branch analysis for '{BranchName}'", branchName);
				}

				File.WriteAllText(ArtifactsDirectory / "BranchName.txt", branchName);

				string configText = $$"""
				                      {
				                      	"stryker-config": {
				                      		"project-info": {
				                      			"name": "github.com/aweXpect/aweXpect.Web",
				                      			"module": "{{project.Key.Name}}",
				                      			"version": "{{branchName}}"
				                      		},
				                      		"test-projects": [
				                      			{{string.Join(",\n\t\t\t", project.Value.Select(PathForJson))}}
				                      		],
				                      		"project": {{PathForJson(project.Key)}},
				                      		"target-framework": "net8.0",
				                      		"since": {
				                      			"target": "main",
				                      			"enabled": {{(BranchName != "main").ToString().ToLowerInvariant()}},
				                      			"ignore-changes-in": [
				                      				"**/.github/**/*.*"
				                      			]
				                      		},
				                      		"mutation-level": "Advanced"
				                      	}
				                      }
				                      """;
				File.WriteAllText(configFile, configText);
				Log.Debug($"Created '{configFile}':{Environment.NewLine}{configText}");

				string arguments =
					$"-f \"{configFile}\" -O \"{strykerOutputDirectory}\" -r \"Markdown\" -r \"cleartext\" -r \"json\"";

				string executable = EnvironmentInfo.IsWin ? "dotnet-stryker.exe" : "dotnet-stryker";
				IProcess process = ProcessTasks.StartProcess(
						Path.Combine(toolPath, executable),
						arguments,
						Solution.Directory)
					.AssertWaitForExit();
				if (process.ExitCode != 0)
				{
					Assert.Fail(
						$"Stryker did not execute successfully for {project.Key.Name}: (exit code {process.ExitCode}).");
				}

				MutationCommentBody += Environment.NewLine + CreateMutationCommentBody(project.Key.Name);
			}
		});

	Target MutationComment => _ => _
		.After(MutationTestExecution)
		.OnlyWhenDynamic(() => GitHubActions.IsPullRequest)
		.Executes(() =>
		{
			int? prId = GitHubActions.PullRequestNumber;
			Log.Debug("Pull request number: {PullRequestId}", prId);
			if (string.IsNullOrWhiteSpace(MutationCommentBody))
			{
				return;
			}

			string body = "## :alien: Mutation Results"
			              + Environment.NewLine
			              + $"[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FaweXpect%2FaweXpect.Web%2Fpull/{prId}/merge)](https://dashboard.stryker-mutator.io/reports/github.com/aweXpect/aweXpect.Web/pull/{prId}/merge)"
			              + Environment.NewLine
			              + MutationCommentBody;
			File.WriteAllText(ArtifactsDirectory / "PR_Comment.md", body);

			if (prId != null)
			{
				File.WriteAllText(ArtifactsDirectory / "PR.txt", prId.Value.ToString());
			}
		});

	Target MutationTestDashboard => _ => _
		.After(MutationTestExecution)
		.Executes(async () =>
		{
			await "MutationTests".DownloadArtifactTo(ArtifactsDirectory, GithubToken);

			Dictionary<Project, Project[]> projects = new()
			{
				{
					Solution.aweXpect_Web, [
						Solution.Tests.aweXpect_Web_Tests,
						Solution.Tests.aweXpect_Web_Internal_Tests,
						Solution.Tests.aweXpect_Web_Samples_Tests,
					]
				},
			};
			string apiKey = Environment.GetEnvironmentVariable("STRYKER_DASHBOARD_API_KEY");
			string branchName = File.ReadAllText(ArtifactsDirectory / "BranchName.txt");
			foreach (KeyValuePair<Project, Project[]> project in projects)
			{
				string reportComment =
					File.ReadAllText(ArtifactsDirectory / "Stryker" / "reports" / "mutation-report.json");
				using HttpClient client = new();
				client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
				// https://stryker-mutator.io/docs/General/dashboard/#send-a-report-via-curl
				await client.PutAsync(
					$"https://dashboard.stryker-mutator.io/api/reports/github.com/aweXpect/aweXpect.Web/{branchName}?module={project.Key.Name}",
					new StringContent(reportComment, new MediaTypeHeaderValue("application/json")));
			}

			if (File.Exists(ArtifactsDirectory / "PR.txt"))
			{
				string prNumber = File.ReadAllText(ArtifactsDirectory / "PR.txt");
				Log.Debug("Pull request number: {PullRequestId}", prNumber);
				string body = File.ReadAllText(ArtifactsDirectory / "PR_Comment.md");
				if (int.TryParse(prNumber, out int prId))
				{
					GitHubClient gitHubClient = new(new ProductHeaderValue("Nuke"));
					Credentials tokenAuth = new(GithubToken);
					gitHubClient.Credentials = tokenAuth;
					IReadOnlyList<IssueComment> comments =
						await gitHubClient.Issue.Comment.GetAllForIssue("aweXpect", "aweXpect.Web", prId);
					long? commentId = null;
					Log.Information($"Found {comments.Count} comments");
					foreach (IssueComment comment in comments)
					{
						if (comment.Body.Contains("## :alien: Mutation Results"))
						{
							Log.Information($"Found comment: {comment.Body}");
							commentId = comment.Id;
						}
					}

					if (commentId == null)
					{
						Log.Information($"Create comment:\n{body}");
						await gitHubClient.Issue.Comment.Create("aweXpect", "aweXpect.Weg", prId, body);
					}
					else
					{
						Log.Information($"Update comment:\n{body}");
						await gitHubClient.Issue.Comment.Update("aweXpect", "aweXpect.Web", commentId.Value, body);
					}
				}
			}
		});

	string CreateMutationCommentBody(string projectName)
	{
		string[] fileContent = File.ReadAllLines(ArtifactsDirectory / "Stryker" / "reports" / "mutation-report.md");
		StringBuilder sb = new();
		sb.AppendLine($"### {projectName}");
		sb.AppendLine("<details>");
		sb.AppendLine("<summary>Details</summary>");
		sb.AppendLine();
		int count = 0;
		foreach (string line in fileContent.Skip(1))
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}

			if (line.StartsWith("#"))
			{
				if (++count == 1)
				{
					sb.AppendLine();
					sb.AppendLine("</details>");
					sb.AppendLine();
				}

				sb.AppendLine("##" + line);
				continue;
			}

			if (count == 0 &&
			    line.StartsWith("|") &&
			    line.Contains("| N\\/A"))
			{
				continue;
			}

			sb.AppendLine(line);
		}

		string body = sb.ToString();
		return body;
	}

	static string PathForJson(Project project) => $"\"{project.Path.ToString().Replace(@"\", @"\\")}\"";
}
