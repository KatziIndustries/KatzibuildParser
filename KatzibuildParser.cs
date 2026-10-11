using System.Runtime.InteropServices;
using System.Text;

public static class KatzibuildParser
{
    private static readonly string[] InstructionSets = [
        "build",
        "install",
        "uninstall",
    ];

    public static Katzibuild Parse(string katzibuildString)
    {
        string[] lines = katzibuildString.Split('\n');
        if (lines.Length == 0) throw new Exception("KATZIBUILD was empty");

        Katzibuild katzibuild = new()
        {
            BuildInstructions = null,
            InstallInstructions = null,
            UninstallInstructions = null
        };

        string? currentInstructionSet = null;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd();

            if (line == string.Empty)
                continue;

            if (line.EndsWith(':'))
            {
                string possibleInstructionSet = line[0..(line.Length - 1)];
                if (InstructionSets.Contains(possibleInstructionSet))
                {
                    currentInstructionSet = possibleInstructionSet;
                    continue;
                }
            }

            if (line.StartsWith(" "))
            {
                if (currentInstructionSet == null || !line.StartsWith("    "))
                {
                    throw new Exception($"No indentation expected at line {i + 1}");
                }

                katzibuild = AddToInstructionSet(katzibuild, currentInstructionSet, line);
                continue;
            }

            int equalSignIndex = line.IndexOf('=');

            if (equalSignIndex == -1)
            {
                throw new Exception($"Unexpected token on line {i + 1}");
            }

            if (equalSignIndex + 1 == line.Length)
            {
                throw new Exception($"KATZIBUILD key didn't have a value on line {i + 1}");
            }

            string key = line[0..equalSignIndex];
            string value = line[(equalSignIndex + 1)..line.Length];

            katzibuild = SetKeyValue(katzibuild, key, value);
        }

        return katzibuild;
    }

    private static Katzibuild AddToInstructionSet(Katzibuild katzibuild, string instructionSet, string value)
    {
        // Trim the value at the beginning to remove the white space characters
        value = value.Trim();

        switch (instructionSet)
        {
            case "build":
                katzibuild.BuildInstructions ??= [];
                katzibuild.BuildInstructions.Add(value);
                break;

            case "install":
                katzibuild.InstallInstructions ??= [];
                katzibuild.InstallInstructions.Add(value);
                break;

            case "uninstall":
                katzibuild.UninstallInstructions ??= [];
                katzibuild.UninstallInstructions.Add(value);
                break;
        }

        return katzibuild;
    }

    private static Katzibuild SetKeyValue(Katzibuild katzibuild, string key, string value)
    {
        switch (key)
        {
            case "type":
                if (!Enum.TryParse(value, false, out KatzibuildType katzibuildType))
                    throw new Exception($"KATZIBUILD type '{value}' isn't a valid KATZIBUILD type, valid types are: {Enum.GetNames<KatzibuildType>().Aggregate((a, b) => $"{a}, {b}")}");

                katzibuild.Type = katzibuildType;
                break;

            case "name":
                katzibuild.Name = value;
                break;

            case "description":
                if (!value.StartsWith('"') || !value.EndsWith('"'))
                    throw new Exception($"KATZIBUILD description didn't start and end with quotation marks");

                katzibuild.Description = value[1..(value.Length - 1)];
                break;

            case "version":
                katzibuild.Version = value;
                break;

            case "url":
                if (!value.StartsWith('"') || !value.EndsWith('"'))
                    throw new Exception($"KATZIBUILD Url didn't start and end with quotation marks");

                katzibuild.Url = value[1..(value.Length - 1)];
                break;

            case "dependencies":
                if (!value.StartsWith('[') || !value.EndsWith(']'))
                    throw new Exception($"KATZIBUILD Url didn't start and end with block parentheses []");

                katzibuild.Dependencies = ParseDependencies(value);
                break;

            default:
                throw new Exception($"key '{key} is not a valid KATZIBUILD key'");
        }

        return katzibuild;
    }

    private static string[]? ParseDependencies(string dependencies)
    {
        // Return null because of an empty dependency array
        if (dependencies.Length == 2)
            return null;

        List<string> entries = [];

        // Start at index 1 because of the [ at the start
        StringBuilder stringBuilder = new();
        bool collectingWord = false;

        for (int i = 1; i < dependencies.Length; i++)
        {
            if (dependencies[i] == '"')
            {
                if (collectingWord)
                {
                    if (stringBuilder.Length == 0)
                    {
                        throw new Exception($"Empty dependency strings are not allowed");
                    }

                    entries.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                    collectingWord = false;
                }
                else
                {
                    collectingWord = true;
                }
            }
            else if (collectingWord)
            {
                stringBuilder.Append(dependencies[i]);
            }
        }

        return [.. entries];
    }

    public static Katzibuild FromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"KATZIBUILD at {filePath} could not be found");
        }

        string fileContent = File.ReadAllText(filePath);

        return Parse(fileContent);
    }
}
