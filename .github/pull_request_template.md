### Description

_Please fill this section with a description what the pull request is trying to address._

### Changes made

_Please write down a short description of what changes were made._

### Notes

_Please write down any additional notes, remove the section if not applicable._

### Checklist

Before review:

- [ ] Changelog entry added under `Unreleased` section.
    - Explains the change in `Modified`, `Fixed`, `Added` sections.
    - For API change contains an example snippet and/or migration example.
    - If UI or rendering results applies, include screenshots.
    - FogBugz ticket attached, example `([case %number%](https://issuetracker.unity3d.com/issues/...))`.
    - FogBugz is marked as `Resolved` with `next` release version correctly set.
- [ ] Tests added/changed, if applicable.
    - Functional tests.
    - Performance tests.
    - Integration tests.
- [ ] All Tests passed.
- [ ] Docs for new/changed.
    - XmlDoc cross references are set correctly.
    - Added explanation how the API works.
    - Usage code examples added.
- [ ] The branch name has the respective prefix.
    - `bug/` Fixing a bug
    - `feature/` New feature implementation
    - `perf/` Performance improvement
    - `refactor/` A code change that neither fixes a bug nor adds a feature
    - `doc/` Added documentation
    - `test/` Added Unit Tests
    - `build/` Changes that affect the build system or external dependencies
    - `ci/` Changes to our CI configuration files and scripts
    - `style/` Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- [ ] Coding Standards are respected.
- [ ] Rebase the branch if possible.

After review:

- [ ] Squash and Merge
    - If no merge commits are between commits
    - Don't squash commits when they are easier to comprehend the changes when categorized.
