import os
import sys
import subprocess
import shutil

script_path = os.path.abspath(__file__).replace("run.py", "")
examples = [example for example in os.listdir(script_path) if os.path.isdir(os.path.join(script_path, example))]

# Get input project name from user from args
example_name = str(sys.argv[1]).strip().lower()

# Check if project exists
if example_name not in examples:
    print(f"Example {example_name} not found")
    print(f"Available examples: {examples}")
    sys.exit(1)

# Get path to repo root
project_root = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))

cli_path = f"{project_root}/src/CLI/CSynth.CLI.csproj"
example_path = f"{project_root}/examples/{example_name}"
example_dll_path = f"{example_path}/bin/Release/net8.0/{example_name}.dll"
output_path = f"{example_path}/{example_name}.lua"

# Build example and wait for it to finish
os.system(f"dotnet build -c Release {example_path}")

# Run example and capture stdout
result = subprocess.run(["dotnet", "run", "--project", cli_path, "--", example_dll_path], capture_output=True, text=True)

if result.returncode != 0:
    print(result.stderr)
    sys.exit(1)

# Write to example output
with open(output_path, 'w') as f:
    f.write(result.stdout)

print(f"Output written to {output_path}")    

# if there's a output.txt file, try compare lua output with it
output_txt_path = f"{example_path}/output.txt"

if os.path.exists(output_txt_path):
    with open(output_txt_path, 'r') as f:
        expected_output = f.read()

    # run example.lua and capture stdout
    lune_executable = shutil.which("lune")

    if lune_executable:
        args = [lune_executable, "run", output_path]
        result = subprocess.run(args, capture_output=True, text=True)
    else:
        print("No lune executable found in path")
        sys.exit(1)
    
    if result.returncode != 0:
        print(f"Error running {example_name}.lua!")
        print(result.stderr)
        sys.exit(1)
    
    if result.stdout.strip() == expected_output.strip():
        print(f"{example_name}.lua output matches expected output")
    else:
        print(f"{example_name}.lua output does not match expected output")
        print("Expected:")
        print(expected_output)
        print("Actual:")
        print(result.stdout)
else:
    print(f"No output.txt file found for {example_name},  won't compare output")

