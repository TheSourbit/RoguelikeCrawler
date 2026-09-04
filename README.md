# RoguelikeCrawler

## Run tests without opening Godot

The tests use GoDotTest and run inside the normal game project. Run the commands below from the repository root; they use Godot's headless mode and do not open the editor.

### Requirements

- Godot 4.7.2 .NET/Mono, available as `godot` on your `PATH`
- .NET 10 SDK, available as `dotnet` on your `PATH`

### First run

Restore the project and build the test-enabled Debug configuration:

```sh
dotnet restore
dotnet build
```

### Run the complete test suite

```sh
godot --headless --path . --run-tests --quit-on-finish
```

The process exits when the tests finish. A successful run ends with a result similar to:

```text
Test results: Passed: 7 | Failed: 0 | Skipped: 0
```

If Godot is not named `godot` on your system, replace `godot` with the path to your .NET/Mono Godot executable.

### Run selected tests

GoDotTest accepts a test class or test name as the value of `--run-tests`:

```sh
godot --headless --path . --run-tests=ShadowcastingSmoke --quit-on-finish
godot --headless --path . --run-tests=ShadowcastingSmoke.OpenFieldCoverage --quit-on-finish
```

Run `dotnet build` again after changing the C# source before running the tests.
