#!/usr/bin/env bash
set -euo pipefail

# Cross-platform build script for SecureHttpClient
# Usage: ./build.sh [clean|restore|build|test|pack|all]

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
LIB_NAME="SecureHttpClient"
CONFIGURATION="${CONFIGURATION:-Release}"
VERSION=$(cat "$ROOT_DIR/version.txt")

echo "======================================================================"
echo "Building $LIB_NAME v$VERSION"
echo "Configuration: $CONFIGURATION"
echo "======================================================================"

function clean() {
    echo "Cleaning..."
    dotnet clean "$ROOT_DIR/$LIB_NAME/$LIB_NAME.csproj" -c "$CONFIGURATION" -v m
}

function restore() {
    echo "Restoring dependencies..."
    dotnet restore "$ROOT_DIR/$LIB_NAME/$LIB_NAME.csproj"
}

function build() {
    echo "Building..."
    # Build for net10.0 only (cross-platform)
    dotnet build "$ROOT_DIR/$LIB_NAME/$LIB_NAME.csproj" \
        -f net10.0 \
        -c "$CONFIGURATION" \
        -v m \
        -p:Version="$VERSION" \
        -p:AssemblyVersion="$VERSION" \
        -p:AssemblyFileVersion="$VERSION" \
        --no-restore
}

function test() {
    echo "Running tests..."
    dotnet test "$ROOT_DIR/$LIB_NAME.Test/$LIB_NAME.Test.csproj" \
        -c "$CONFIGURATION" \
        -v m \
        --no-build
}

function pack() {
    echo "Creating NuGet package..."
    dotnet pack "$ROOT_DIR/$LIB_NAME/$LIB_NAME.csproj" \
        -c "$CONFIGURATION" \
        -v m \
        --no-build \
        -o "$ROOT_DIR" \
        -p:PackageVersion="$VERSION" \
        -p:IncludeSymbols=true \
        -p:SymbolPackageFormat=snupkg
}

function build_all() {
    clean
    restore
    build
    echo "Build completed successfully!"
}

# Main script logic
case "${1:-all}" in
    clean)
        clean
        ;;
    restore)
        restore
        ;;
    build)
        build
        ;;
    test)
        test
        ;;
    pack)
        pack
        ;;
    all)
        build_all
        ;;
    *)
        echo "Usage: $0 [clean|restore|build|test|pack|all]"
        exit 1
        ;;
esac

echo "======================================================================"
echo "Done!"
echo "======================================================================"
