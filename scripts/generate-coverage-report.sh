#!/bin/bash

# Simple Code Coverage Report Script
set -e

echo "🔍 Generating Code Coverage Report..."

# Configuration
COVERAGE_DIR="./TestResults"
REPORT_DIR="./CoverageReports"

# Clean and create directories
echo "🧹 Cleaning previous results..."
rm -rf "$COVERAGE_DIR" "$REPORT_DIR" 2>/dev/null || true
mkdir -p "$COVERAGE_DIR" "$REPORT_DIR"

# Run tests with coverage
echo "🧪 Running tests with coverage..."
dotnet test \
    --configuration Release \
    --collect:"XPlat Code Coverage" \
    --settings tests/BlogApp.UnitTests/coverlet.runsettings \
    --verbosity normal

# Find coverage file
COVERAGE_FILE=$(find "./tests/BlogApp.UnitTests/TestResults" -name "coverage.cobertura.xml" | head -1)

if [ -z "$COVERAGE_FILE" ]; then
    echo "❌ Coverage file not found. Tests may have failed."
    exit 1
fi

echo "📊 Found coverage file: $COVERAGE_FILE"

# Generate HTML report
echo "📊 Generating HTML report..."
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.0 2>/dev/null || true

reportgenerator \
    -reports:"$COVERAGE_FILE" \
    -targetdir:"$REPORT_DIR/HTML" \
    -reporttypes:"Html" \
    -verbosity:Info

# Display results
echo "✅ Coverage report generated successfully!"
echo "📁 Report location: $REPORT_DIR/HTML/index.html"

# Open report on macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
    open "$REPORT_DIR/HTML/index.html"
fi
