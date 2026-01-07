# Contributing to SecureHttpClient

Thank you for your interest in contributing to SecureHttpClient! This document provides guidelines and instructions for contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Pull Request Process](#pull-request-process)

## Code of Conduct

This project adheres to the Contributor Covenant [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to contact@zitch.com.

## Getting Started

SecureHttpClient is a cross-platform HttpClientHandler library with enhanced security features for .NET applications targeting Android, iOS, and Windows platforms.

### Prerequisites

- Visual Studio 2022 18.1+ or Visual Studio Code
- .NET 10.0 SDK or later
- For mobile development:
  - Android SDK for Android development
  - Xcode for iOS development (macOS only)
  - MAUI workloads installed via `dotnet workload install maui`

## Development Setup

1. **Fork and Clone**
   ```bash
   git clone https://github.com/YOUR-USERNAME/SecureHttpClient.git
   cd SecureHttpClient
   ```

2. **Install Workloads** (for mobile development)
   ```bash
   dotnet workload restore
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the Solution**
   ```bash
   # For Windows/.NET only (without mobile workloads)
   dotnet build SecureHttpClient/SecureHttpClient.csproj -f net10.0
   
   # For full build (requires workloads)
   dotnet build
   ```

## How to Contribute

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When creating a bug report, include:

- A clear and descriptive title
- Steps to reproduce the issue
- Expected behavior
- Actual behavior
- Platform and version information
- Code samples or test cases if applicable

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, include:

- A clear and descriptive title
- Detailed description of the proposed feature
- Rationale for why this enhancement would be useful
- Examples of how the feature would be used

### Code Contributions

1. **Find or Create an Issue**: Before starting work, check if there's an existing issue or create one to discuss your changes.

2. **Create a Branch**: Create a feature branch from the main branch.
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make Changes**: Implement your changes following the coding standards below.

4. **Write Tests**: Add or update tests to cover your changes.

5. **Test Your Changes**: Ensure all tests pass and your code works on relevant platforms.

6. **Commit Your Changes**: Write clear, descriptive commit messages.
   ```bash
   git commit -m "Add feature: brief description"
   ```

7. **Push and Create PR**: Push your branch and create a pull request.

## Coding Standards

### General Guidelines

- Follow C# coding conventions and .NET design guidelines
- Use C# 14.0 language features where appropriate
- Maintain consistency with existing code style
- Write self-documenting code with meaningful names
- Add XML documentation comments for all public APIs
- Keep methods focused and concise (single responsibility principle)

### Code Style

- Use 4 spaces for indentation (not tabs)
- Place opening braces on new lines (Allman style)
- Use `var` when the type is obvious
- Prefer string interpolation over concatenation
- Use expression-bodied members where appropriate
- Follow naming conventions:
  - PascalCase for classes, methods, properties
  - camelCase for local variables and parameters
  - _camelCase for private fields
  - UPPER_CASE for constants

### Platform-Specific Code

- Use preprocessor directives for platform-specific implementations:
  ```csharp
  #if __ANDROID__
  // Android-specific code
  #elif __IOS__
  // iOS-specific code
  #else
  // Windows/.NET code
  #endif
  ```

### Documentation

- Add XML documentation comments for:
  - All public classes, interfaces, and members
  - Complex internal methods
  - Non-obvious code sections
- Include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags as appropriate
- Keep comments up-to-date with code changes

## Testing

### Running Tests

Tests are located in the `SecureHttpClient.Test` project and use xUnit as the testing framework.

```bash
# Run all tests
dotnet test SecureHttpClient.Test/SecureHttpClient.Test.csproj

# Run specific test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

### Writing Tests

- Write tests for all new features and bug fixes
- Follow the Arrange-Act-Assert (AAA) pattern
- Use descriptive test method names that explain what is being tested
- Test both success and failure scenarios
- Include platform-specific tests when appropriate
- Use `SkippableFact` attribute for tests that only work on certain platforms

### Test Coverage

- Aim for high test coverage of public APIs
- Include integration tests for critical security features (certificate pinning, TLS)
- Test edge cases and error conditions

## Pull Request Process

1. **Update Documentation**: Update README.md or other documentation if needed.

2. **Update CHANGELOG**: Add an entry to CHANGELOG.md describing your changes.

3. **Ensure Tests Pass**: All tests must pass before the PR can be merged.

4. **Code Review**: Address any feedback from maintainers during code review.

5. **Squash Commits**: You may be asked to squash commits before merging.

6. **Merge**: Once approved, a maintainer will merge your PR.

### PR Checklist

- [ ] Code follows the project's coding standards
- [ ] XML documentation added for public APIs
- [ ] Tests added or updated to cover changes
- [ ] All tests pass
- [ ] CHANGELOG.md updated
- [ ] Documentation updated if needed
- [ ] No unrelated changes included
- [ ] Commit messages are clear and descriptive

## Certificate Pinning Testing

When testing certificate pinning features:

1. Use test domains with stable certificates or mock servers
2. Update certificate pins in tests when certificates are rotated
3. Document the process for obtaining/updating pins in test comments
4. Test both successful validation and expected failures

## Questions or Problems?

If you have questions or run into problems, please:

1. Check existing documentation and issues
2. Ask in a GitHub issue or discussion
3. Contact the maintainers at contact@zitch.com

## License

By contributing to SecureHttpClient, you agree that your contributions will be licensed under the MIT License.

## Recognition

Contributors will be recognized in the project's release notes and commit history. Thank you for helping improve SecureHttpClient!
