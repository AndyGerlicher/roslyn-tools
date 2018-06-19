// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#r "GithubMergeTool.dll"

#load "auth.csx"

using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

private static TraceWriter Log = null;

private static async Task MakeGithubPr(
    GithubMergeTool.GithubMergeTool gh,
    string repoOwner,
    string repoName,
    string srcBranch,
    string destBranch,
    bool addAutoMergeLabel = false)
{
    Log.Info($"Merging {repoName} from {srcBranch} to {destBranch}");

    var (prCreated, error) = await gh.CreateMergePr(repoOwner, repoName, srcBranch, destBranch, addAutoMergeLabel);

    if (prCreated)
    {
        Log.Info("PR created successfully");
    }
    else if (error == null)
    {
        Log.Info("PR creation skipped. PR already exists or all commits are present in base branch");
    }
    else
    {
        Log.Error($"Error creating PR. GH response code: {error.StatusCode}");
        Log.Error(await error.Content.ReadAsStringAsync());
    }
}

private static async Task RunAsync()
{
    var gh = new GithubMergeTool.GithubMergeTool("dotnet-bot@users.noreply.github.com", await GetSecret("dotnet-bot-github-auth-token"));
    var config = XDocument.Parse("config.xml").Root;
    foreach (var repo in config.Elements("repo"))
    {
        var owner = repo.Attribute("owner").Value;
        var name = repo.Attribute("name").Value;
        foreach (var merge in repo.Elements("merge"))
        {
            var fromBranch = merge.Attribute("from").Value;
            var toBranch = merge.Attribute("to").Value;
            await MakeGithubPr(gh, owner, name, fromBranch, toBranch); // TODO: addAutoMergeLabel
        }
    }
}

public static void Run(TimerInfo every12Hours, TraceWriter log)
{
    Log = log;

    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

    RunAsync().GetAwaiter().GetResult();
}
