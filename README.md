# Project Setup and Configuration and details about the MyChatApp and Copilot Project

## Overview

**MyChatApp** is a code evaluation challenge designed to help engineers gain a deeper understanding of the capabilities of Copilot Agent Mode. This challenge is implemented as a C# Blazor application and includes precreated issues that serve as stories for the work.

## Purpose

The primary goal of MyChatApp is to provide a hands-on experience with Copilot Agent Mode, allowing engineers to explore its features and functionalities in a practical setting. By working through the precreated issues, participants will be able to:

- Understand how Copilot Agent Mode can assist in code development and problem-solving.
- Gain insights into integrating Copilot with existing workflows.
- Evaluate the efficiency and effectiveness of Copilot in real-world scenarios.

## Features

- **Precreated Issues**: A set of predefined issues that act as stories, guiding participants through various tasks and challenges.
- **C# Blazor Application**: A robust and scalable application framework that showcases the capabilities of Copilot Agent Mode.
- **Interactive Learning**: Engaging tasks that encourage exploration and experimentation with Copilot's features.

## Getting Started

To get started with MyChatApp, follow these steps:

1. Clone the repository.
2. Review the README and familiarize yourself with the project structure.
3. Begin working on the precreated issues, using Copilot Agent Mode to assist you.

## Additional Resources

[Authors](AUTHORS.md) <br />
[ChangeLog](CHANGELOG.md) <br />
[Contributing](CONTRIBUTING.md) <br /><br />
[Project Documentation](Docs/README.md)

## Project Dependencies
### Required
- .NET
- Visual Studio
- SQL Server Express

### Recommended
- Docker Desktop
- Visual Studio Code
- SQL Server Management Studio

## Getting Started
### Setting up the project
1. Clone the repository to your local machine.
2. Locate the file docker_setup.ps1 in the root of the project and run this in PowerShell which will setup the Docker Containers and Dependencies.

### Using SQL Server Express
1. To get started with this project, you will need to clone the repository and then open the solution in Visual Studio.
2. Once the solution is open, you will want to build the solution. This will download all of the NuGet packages that are required or the project.
3. Open the Database.Example project and build the project then publish using the "StartupExample.publish.xml" profile.
4. There should be no need to update the secrets.json file with the connection string as it should be configured in the appsettings.json file for SQL Express.

### Using Docker Containers
1. Start Visual Studio and open the solution.
2. Open the Database.Example project and build the project then publish using the "StartupExample.Docker.publish.xml" profile. Using the default password of "P@ssword123!".
3. MailHog was added as an email trap.
4. Open Telemetry was added to the project to help with debugging and development and can be found [here](http://localhost:10150/) after starting the docker containers.
   1. Watch the following video for why and how its used: [The Logging Everyone Should Be Using in .NET](https://www.youtube.com/watch?v=MHJ0BHfWhRw)

### Running the Applications
1. Once the project has been built and the database has been created, you can run any of the applications.

## CodeSpaces

If you are using CodeSpaces you'll want to update your container git configuration profile. Be sure to update with your appropriate name and email details.

### GitHub Configuration
```
git config --global user.name "Your Name"
git config --global user.email "youremail@yourdomain.com"
```

## Installing Resources

This document outlines the resources generally needed to do development on this project. Some resources are not critical but recommended.

### DotNet SDKs

✅ Required Resource ✅

This includes .NET 8 and .NET 9.

```
winget install Microsoft.DotNet.SDK.8
winget install Microsoft.DotNet.SDK.9
```

### Windows Terminal Console Support

👍 Recommended 👍

This utility provides windows a Unix like tabbed console window manager.

```
winget install Microsoft.WindowsTerminal
```

### Latest version of PowerShell

✅ Required Resource ✅

DevOps customized commands are written using the latest version of PowerShell.

```
winget install Microsoft.PowerShell
```

### Azure toolkit

👍 Recommended 👍

These tools provide CLI support for interacting with and managing Azure resources from your console. Some of these tools may be required depending on some of the development work required such as FunctionsCoreTools if doing function app development.

```
winget install Microsoft.AzureCLI
winget install Microsoft.Azd
winget install Microsoft.Azure.FunctionsCoreTools
```

### CommandLine Git Client

✅ Required Resource ✅

This adds the windows GIT client to interact with git based repositories.

```
winget install Git.Git
```

### Visual Git Clients

👍 Recommended 👍

These are two recommended GUI wrapper clients for the git Cli. These are not required but maybe helpful in managing repository changes and visualizing history and branching better. Atlassian's tool works very well at providing a history tree view for tracking changes. Both Visual Studio and VS Code have git support making these tools optional.

```
winget install Atlassian.Sourcetree
winget install GitHub.GitHubDesktop
```

### Primarily used for Git Merge Conflicts, compares files and folders

👍 Recommended 👍

When performing merging of branches, the BeyondCompare tool has been fantastic. This tool also supports comparisons between folders making it easier to perform diffs between two large directory structures. This tool also integrates well with Atlassian Source Tree.

```
winget install ScooterSoftware.BeyondCompare.5
```

### Azure

If you need to work with Azure Blob Storage this Visual GUI tool provides easy access and management of content stored in an Storage resource in Azure.

```
winget install Microsoft.Azure.StorageEmulator
winget install Microsoft.Azure.StorageExplorer
```

### Http Gui Client

👍 Recommended 👍

You can install either Postman or Bruno

```
winget install Postman.Postman
winget install Bruno.Bruno
```

### Local Database Engine, SQL Server Express

👍 Recommended 👍

Having a local instance of SQL Server can be helpful for development purposes.

```
winget install Microsoft.SQLServer.2022.Express
```

### IDE and Text Editors

👍 Recommended 👍

Our primary development must use Visual Studio and / or Visual Studio Code

```
winget install Microsoft.VisualStudio.2022.Enterprise
winget install Microsoft.VisualStudioCode
```

👍 Recommended 👍

Having the ability to query the database is required, however, SQL Server Management Studio is recommended as the primary tool for doing so.
Notepad++ is a lightweight text editor that is very handy.

```
winget install Microsoft.SQLServerManagementStudio
winget install Notepad++.Notepad++
```

### VS 2022 Addon Tools Extensions

👍 Recommended 👍

Highly recommended. This tool should be used periodically for its refactor and C# linting fix up capabilities for C# code.

JetBrains.Toolbox is a centralized application manager that simplifies the installation and management of JetBrains IDEs and tools. JetBrains.ReSharper is a Visual Studio extension that enhances code quality and productivity with advanced refactoring, navigation, and code analysis features.

```
winget install JetBrains.Toolbox
winget install JetBrains.ReSharper
```

### Docker / Container Services

👍 Recommended 👍

This resource provides local development tools and resources will speed up developer setup and configuration and provide diagnostic and debugging resources not previously available. The number of options and tools is near limitless.

```
winget install Docker.DockerDesktop
```

### Update Dotnet workloads

👍 Recommended 👍

It is recommended to periodically run the dotnet workload update command, as this will help keep your local development tools and resources up to date. Running Visual Studio updates do not always update all of the dotnet resources.
If you desire experimenting with Aspire resources it is recommended to install the wasm-tools and aspire workloads.
Lastly, the LibraryManager provides resources for using libman.json configuration files to automatically download web resources such as jQuery, Bootstrap, and other libraries for web development.

```
dotnet workload update
dotnet workload install wasm-tools
dotnet workload install aspire
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

## Getting the project setup

### Configure unsigned PowerShell scripts to run locally

This will update PowerShell permissions settings to grant unsigned scripts access to run locally.

Run as local machine administrator

```
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser
```

### DevContainers

👍 Recommended 👍 <br />

A DevContainer is a development environment defined by a Docker container, which includes all necessary tools, libraries, and dependencies for a project. It ensures a consistent development setup across different machines, simplifies onboarding, and enhances productivity by providing a pre-configured environment that mirrors production settings. DevContainers are commonly used with Visual Studio Code to streamline development workflows and improve collaboration.

[DevContainer Documentation](https://code.visualstudio.com/docs/devcontainers/containers)

⚠️ Helpful resources ⚠️ <br />

Container Images

- docker.io/library/redis
- ghcr.io/azure/azure-dev/azd
- ghcr.io/devcontainers/features/common-utils
- ghcr.io/devcontainers/features/docker-in-docker
- ghcr.io/devcontainers/features/dotnet
  - aspire
  - wasm-tools
- ghcr.io/devcontainers/features/github-cli
- ghcr.io/devcontainers/features/powershell
- ghcr.io/devcontainers/features/node
- mcr.microsoft.com/azure-storage/azurite
- mcr.microsoft.com/devcontainers/dotnet:1-9.0
- mcr.microsoft.com/dotnet/sdk
- mcr.microsoft.com/dotnet/aspnet
- mcr.microsoft.com/mssql/server
