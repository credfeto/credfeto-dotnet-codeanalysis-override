# credfeto-dotnet-codeanalysis-override
Dotnet code analysis override tool

dotnet tool to allow configured overrides for CodeAnalysis.ruleset files allowing a lower set of analysis rules for development and pre-release builds compared to release builds.  This allows developers to work with rules like being able to add TODO's and comment out code while developing, but prohibits them in builds in pull requests and release builds.

## Installing

```bash
dotnet tool install Credfeto.DotNet.Code.Analysis.Overrides.Cmd
```

## Usage

```bash
 dotnet code-analysis ruleset \
                --ruleset "CodeAnalysis.ruleset" \
                --changes "release.rule-settings.json"
```

### Sample override rules file

```json
[
  {
    "ruleSet": "SonarAnalyzer.CSharp",
    "rule": "S125",
    "state": "Error",
    "description": "No commented out code."
  },
  {
    "ruleSet": "FunFair.CodeAnalysis",
    "rule": "FFS0008",
    "state": "Error",
    "description": "Use of non-whitelisted #pragma warning"
  },
  {
    "ruleSet": "FunFair.CodeAnalysis",
    "rule": "FFS0040",
    "state": "None",
    "description": "Filename must match typename"
  },
  {
    "ruleSet": "FunFair.CodeAnalysis",
    "rule": "FFS0042",
    "state": "None",
    "description": "Do not have TODO's in SuppressMessage"
  },
  {
    "ruleSet": "Meziantou.Analyzer",
    "rule": "MA0026",
    "state": "None",
    "description": "Do not leave TODO's scattered around"
  },
  {
    "ruleSet": "oslynator.Analyzers",
    "rule": "MA0026",
    "state": "None",
    "description": "File With no code"
  }
]
```

## Build Status

| Branch  | Status                                                                                                                                                                                                                                |
|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| main    | [![Build: Pre-Release](https://github.com/credfeto/credfeto-dotnet-codeanalysis-override/actions/workflows/build-and-publish-pre-release.yml/badge.svg)](https://github.com/credfeto/credfeto-dotnet-codeanalysis-override/actions/workflows/build-and-publish-pre-release.yml) |
| release | [![Build: Release](https://github.com/credfeto/credfeto-dotnet-codeanalysis-override/actions/workflows/build-and-publish-release.yml/badge.svg)](https://github.com/credfeto/credfeto-dotnet-codeanalysis-override/actions/workflows/build-and-publish-release.yml)             |

## Changelog

View [changelog](CHANGELOG.md)

## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->