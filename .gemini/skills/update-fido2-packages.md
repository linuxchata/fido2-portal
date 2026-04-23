# Skill: Update SharkFido2Version

Automate the process of updating the `SharkFido2Version` in `Directory.Packages.props` by checking NuGet for the latest version.

## Instructions

1. **Search NuGet**: Fetch the latest version from the JSON feed at `https://api.nuget.org/v3-flatcontainer/shark.fido2.core/index.json`. The latest version is typically the last item in the `versions` array.
2. **Read Local Version**: View the `Directory.Packages.props` file in the repository root and identify the current value of the `<SharkFido2Version>` property.
3. **Compare Versions**:
    * If the NuGet version is greater than the local version:
        a. Create a new Git branch named `feature/update-shark-fido2-v[VERSION]`.
        b. Update the `SharkFido2Version` property in `Directory.Packages.props` with the new version number.
        c. Run `dotnet build src/Shark.Fido2.Portal.slnx` to verify the update.
        d. If the build succeeds, commit the change with the message `Update Shark Fido2 packages to version [VERSION]`.
        e. Push the branch (if applicable) and inform the user.
    * If the local version is already up-to-date:
        a. Inform the user that no update is required.

## Target File
`Directory.Packages.props`

## Expected Output
A build-verified commit on a new feature branch updating the package version.
