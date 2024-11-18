import os
import sys
import subprocess
import shutil

script_path = os.path.abspath(__file__).replace("run.py", "")
examples = [example for example in os.listdir(script_path) if os.path.isdir(os.path.join(script_path, example))]

# Get path to repo root
project_root = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
cli_path = f"{project_root}/src/CLI/CSynth.CLI.csproj"

def compile_example(example_name):
    example_path = f"{project_root}/examples/{example_name}"
    example_dll_path = f"{example_path}/bin/Release/net8.0/{example_name}.dll"
    output_path = f"{example_path}/{example_name}.lua"
    
    # Build example and wait for it to finish
    os.system(f"dotnet build -c Release {example_path}")

    # Compile example and capture stdout
    result = subprocess.run(["dotnet", "run", "--project", cli_path, "--", example_dll_path], capture_output=True, text=True)
    
    if result.returncode != 0:
        print(result.stderr)
        sys.exit(1)
    
    # Write to example output
    with open(output_path, 'w') as f:
        f.write(result.stdout)

    print(f"Output written to {output_path}")    

def run_example(example_name):
    example_path = f"{project_root}/examples/{example_name}"
    output_path = f"{example_path}/{example_name}.lua"

    # if there's a output.txt file, try compare lua output with it
    output_txt_path = f"{example_path}/output.txt"
    input_txt_path = f"{example_path}/input.txt"

    expected_output = ""
    input_contents = ""

    if not os.path.exists(output_txt_path):
        print(f"output.txt file not found for {example_name}, skipping comparison")
    else:
        with open(output_txt_path, 'r') as f:
            expected_output = f.read()

    if os.path.exists(input_txt_path):
        with open(input_txt_path, 'r') as f:
            input_contents = f.read()

    # run example.lua and capture stdout
    lune_executable = shutil.which("lune")

    if lune_executable:
        args = [lune_executable, "run", output_path]
        result = subprocess.run(args, input=input_contents, capture_output=True, text=True)
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

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: run.py [example_name/all]")
        sys.exit(1)

    example_name = sys.argv[1]

    if example_name in examples:
        compile_example(example_name)
        run_example(example_name)
    elif example_name == "all":
        for example in examples:
            compile_example(example)
            run_example(example)
    else:
        print(f"Example {example_name} not found")
        sys.exit(1)
