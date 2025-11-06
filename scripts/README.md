# Release Scripts

This directory contains scripts used by the release workflow to automate version updates.

## update-version.sh

Updates version numbers across the project.

**Usage:**
```bash
./scripts/update-version.sh <version>
```

**What it does:**
- Updates `<Version>` in `src/Forge.OData.Client/Forge.OData.Client.csproj`
- Updates `<Version>` in `src/Forge.OData.CLI/Forge.OData.CLI.csproj`
- Updates `ODataClientVersion` constant in `src/Forge.OData.CLI/Commands/AddCommand.cs`
- Updates CHANGELOG.md with intelligent version handling:
  - **For prerelease versions** (e.g., 1.0.0-beta.1): Replaces `[Unreleased]` with the new version
  - **For release versions** (e.g., 1.0.0): Merges `[Unreleased]` and all prereleases of the same version (e.g., 1.0.0-rc.*, 1.0.0-beta.*, 1.0.0-alpha.*) into a single release entry

**Examples:**

*Prerelease version:*
```bash
./scripts/update-version.sh 1.0.0-beta.1
# Only replaces [Unreleased] with [1.0.0-beta.1]
```

*Release version:*
```bash
./scripts/update-version.sh 1.0.0
# Merges [Unreleased], [1.0.0-rc.2], [1.0.0-rc.1], [1.0.0-beta.1], etc. into [1.0.0]
```

**Changelog Merging Example:**

Before running `./scripts/update-version.sh 1.0.0`:
```markdown
## [Unreleased]
- Final fixes

## [1.0.0-rc.1] - 2025-11-05
- Release candidate fixes

## [1.0.0-beta.1] - 2025-11-04
- Beta features
```

After:
```markdown
## [1.0.0] - 2025-11-06
- Final fixes

- Release candidate fixes

- Beta features
```

## get-changelog-entry.sh

Extracts the changelog entry for a specific version.

**Usage:**
```bash
./scripts/get-changelog-entry.sh <version>
```

**What it does:**
- Extracts the content between `[version]` and the next version heading
- Used by the release workflow to populate GitHub release notes

**Example:**
```bash
./scripts/get-changelog-entry.sh 1.0.0
```

## Workflow Integration

These scripts are automatically invoked by the `.github/workflows/release.yml` workflow when creating a new release. You typically don't need to run them manually unless testing the release process.
