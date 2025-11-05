#!/bin/bash

# Script to update version across the project
# Usage: ./scripts/update-version.sh <version>

set -e

if [ -z "$1" ]; then
    echo "Error: Version argument required"
    echo "Usage: $0 <version>"
    exit 1
fi

VERSION=$1

# Validate version format (basic semver check)
if ! [[ $VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.-]+)?$ ]]; then
    echo "Error: Invalid version format. Expected semver format (e.g., 1.0.0 or 1.0.0-beta.1)"
    exit 1
fi

echo "Updating version to $VERSION..."

# Update Forge.OData.Client project version
echo "  Updating Forge.OData.Client.csproj..."
if [[ "$(uname)" == "Darwin" ]]; then
    # BSD sed (macOS) requires an explicit empty backup suffix for in-place edits
    sed -i '' "s|<Version>.*</Version>|<Version>$VERSION</Version>|g" src/Forge.OData.Client/Forge.OData.Client.csproj
else
    # GNU sed (Linux)
    sed -i "s|<Version>.*</Version>|<Version>$VERSION</Version>|g" src/Forge.OData.Client/Forge.OData.Client.csproj
fi

# Update Forge.OData.CLI project version
echo "  Updating Forge.OData.CLI.csproj..."
if [[ "$(uname)" == "Darwin" ]]; then
    # BSD sed (macOS) requires an explicit empty backup suffix for in-place edits
    sed -i '' "s|<Version>.*</Version>|<Version>$VERSION</Version>|g" src/Forge.OData.CLI/Forge.OData.CLI.csproj
else
    # GNU sed (Linux)
    sed -i "s|<Version>.*</Version>|<Version>$VERSION</Version>|g" src/Forge.OData.CLI/Forge.OData.CLI.csproj
fi

# Update README.md installation instructions
echo "  Updating README.md..."
# Update the Forge.OData.CLI installation command (if version is explicitly mentioned)
# For now, the README uses --global without version, so we'll add a note
# Users typically install latest with: dotnet tool install --global Forge.OData.CLI
# But we can update if there are specific version references

# Update CHANGELOG.md - move [Unreleased] to new version
echo "  Updating CHANGELOG.md..."
DATE=$(date +%Y-%m-%d)

# Create a temporary file
TEMP_FILE=$(mktemp)

# Read the changelog and update it
awk -v version="$VERSION" -v date="$DATE" '
/^## \[Unreleased\]/ {
    # Replace the Unreleased header with the new released version header
    print "## [" version "] - " date
    # skip the original Unreleased header line
    next
}
{ print }
' CHANGELOG.md > "$TEMP_FILE"

# Replace the original file
mv "$TEMP_FILE" CHANGELOG.md

echo "Version updated to $VERSION successfully!"
echo ""
echo "Updated files:"
echo "  - src/Forge.OData.Client/Forge.OData.Client.csproj"
echo "  - src/Forge.OData.CLI/Forge.OData.CLI.csproj"
echo "  - CHANGELOG.md"
