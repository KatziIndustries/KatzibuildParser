public struct Katzibuild
{
    public KatzibuildType Type;
    public string Name;
    public string Description;
    public string Version;
    public string Url;
    public string[]? Dependencies;
    public List<string>? BuildInstructions;
    public List<string>? InstallInstructions;
    public List<string>? UninstallInstructions;
}
