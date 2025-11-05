# GitHub Workflows

## Release Workflow

The `release.yml` workflow automates the release process for Forge.OData.Client and Forge.OData.CLI packages.

### How to Use

1. Navigate to the Actions tab in the GitHub repository
2. Select "Create Release" workflow
3. Click "Run workflow"
4. Enter the version number (e.g., `1.0.0` or `1.0.0-beta.1`)
5. Click "Run workflow"

### What It Does

The workflow performs the following steps:

1. **Validates version format** - Ensures the version follows semantic versioning (e.g., 1.0.0, 1.0.0-beta.1)

2. **Creates release branch** - Creates a new branch named `release/<version>`

3. **Updates version numbers** - Updates version in:
   - `src/Forge.OData.Client/Forge.OData.Client.csproj`
   - `src/Forge.OData.CLI/Forge.OData.CLI.csproj`

4. **Updates CHANGELOG.md** - Converts the `[Unreleased]` section to `[<version>] - <date>`

5. **Commits changes** - Commits all version updates

6. **Creates and pushes tag** - Creates a Git tag `v<version>` and pushes it

7. **Extracts changelog** - Extracts the changelog entry for the new version

8. **Creates GitHub Release** - Creates a GitHub release with:
   - Tag: `v<version>`
   - Title: `<version>`
   - Description: Changelog content for this version
   - Prerelease flag: Set automatically if version contains a hyphen (e.g., `-beta`)

9. **Builds and packs NuGet packages** - Builds both projects in Release configuration and creates NuGet packages

10. **Uploads artifacts** - Uploads the `.nupkg` files as workflow artifacts

11. **Creates Pull Request** - Creates a PR to merge the release branch back to `main`

### Version Format

The workflow accepts semantic versioning formats:
- Release versions: `1.0.0`, `2.5.3`, etc.
- Pre-release versions: `1.0.0-beta.1`, `2.0.0-rc.1`, `1.5.0-alpha`, etc.

Pre-release versions (containing a hyphen) will be marked as "prerelease" in GitHub.

### Manual Steps After Running

After the workflow completes:

1. Review and merge the automatically created Pull Request
2. Download the NuGet packages from the workflow artifacts
3. Publish the packages to NuGet.org (this step is manual to prevent accidental publishes)

### Publishing to NuGet

To publish the packages to NuGet.org:

```bash
# Download the artifacts from the workflow run
# Then publish using dotnet nuget push

dotnet nuget push Forge.OData.Client.<version>.nupkg --api-key <your-api-key> --source https://api.nuget.org/v3/index.json
dotnet nuget push Forge.OData.CLI.<version>.nupkg --api-key <your-api-key> --source https://api.nuget.org/v3/index.json
```

### Before Creating a Release

Before running the release workflow:

1. Ensure all changes are merged to `main`
2. Update the `[Unreleased]` section in CHANGELOG.md with all changes for this version
3. Ensure all tests pass
4. Consider any breaking changes and update the version number accordingly (major.minor.patch)
