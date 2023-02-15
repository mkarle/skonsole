﻿using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Reliability;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddConsole();
});

// Get an instance of ILogger
var logger = loggerFactory.CreateLogger<Program>();

var _kernel = Kernel.Build(logger);

// _kernel.Log.LogTrace("KernelSingleton.Instance: adding OpenAI backends");
// _kernel.Config.AddOpenAICompletionBackend("text-davinci-003", "text-davinci-003", EnvVar("OPENAI_API_KEY"));

_kernel.Log.LogTrace("KernelSingleton.Instance: adding Azure OpenAI backends");
_kernel.Config.AddAzureOpenAICompletionBackend(EnvVar("AZURE_OPENAI_DEPLOYMENT_LABEL"), EnvVar("AZURE_OPENAI_DEPLOYMENT_NAME"), EnvVar("AZURE_OPENAI_API_ENDPOINT"), EnvVar("AZURE_OPENAI_API_KEY"));

_kernel.Config.SetRetryMechanism(new PollyRetryMechanism());

var fileOption = new Option<FileInfo?>(
    name: "--file",
    description: "The file to read and display on the console.");

var rootCommand = new RootCommand();
var commitCommand = new Command("commit", "Commit subcommand");
var prCommand = new Command("pr", "Pull Request feedback subcommand");
var prFeedbackCommand = new Command("feedback", "Pull Request feedback subcommand");
var prDescriptionCommand = new Command("description", "Pull Request description subcommand");

rootCommand.SetHandler(async () => await RunCommitMessage(_kernel));
commitCommand.SetHandler(async () => await RunCommitMessage(_kernel));
prCommand.SetHandler(async () => await RunPullRequestDescription(_kernel));
prFeedbackCommand.SetHandler(async () => await RunPullRequestFeedback(_kernel));
prDescriptionCommand.SetHandler(async () => await RunPullRequestDescription(_kernel));

prCommand.Add(prFeedbackCommand);
prCommand.Add(prDescriptionCommand);

rootCommand.Add(commitCommand);
rootCommand.Add(prCommand);

return await rootCommand.InvokeAsync(args);

static async Task RunCommitMessage(IKernel kernel)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "diff --staged",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8
        }
    };
    process.Start();

    string output = process.StandardOutput.ReadToEnd();

    var pullRequestSkill = new PRSkill.PullRequestSkill(kernel);
    var kernelResponse = await kernel.RunAsync(output, pullRequestSkill.GenerateCommitMessage);
    Console.WriteLine(kernelResponse.ToString());
}

static async Task RunPullRequestDescription(IKernel kernel)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "show origin/main..HEAD",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8
        }
    };
    process.Start();

    string output = process.StandardOutput.ReadToEnd();
    var pullRequestSkill = new PRSkill.PullRequestSkill(kernel);
    var kernelResponse = await kernel.RunAsync(output, pullRequestSkill.GeneratePR);
    Console.WriteLine(kernelResponse.ToString());
}

static async Task RunPullRequestFeedback(IKernel kernel)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "show origin/main..HEAD",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8
        }
    };
    process.Start();

    string output = process.StandardOutput.ReadToEnd();

    var pullRequestSkill = new PRSkill.PullRequestSkill(kernel);
    var kernelResponse = await kernel.RunAsync(output, pullRequestSkill.GeneratePullRequestFeedback);
    Console.WriteLine(kernelResponse.ToString());
}

static string EnvVar(string name)
{
    var value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrEmpty(value)) throw new Exception($"Env var not set: {name}");
    return value;
}
