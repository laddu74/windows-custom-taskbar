# Branching and Commit Rules

1. **New Features**: When creating a new feature, always create a new branch from `main` named `feature/<feature-name>`. Once complete, PR and merge back to `main`.
2. **Bug Fixes**: When resolving a bug, always create a new branch from `main` named `bug-fix/<bug-name>`. Once complete, PR and merge back to `main`.
3. **Main Branch**: The `main` branch should always represent the stable, deployable state of the application. Direct commits to `main` should be avoided for feature work.
