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
- Converts `[Unreleased]` section in CHANGELOG.md to `[version] - date`

**Example:**
```bash
./scripts/update-version.sh 1.0.0
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
