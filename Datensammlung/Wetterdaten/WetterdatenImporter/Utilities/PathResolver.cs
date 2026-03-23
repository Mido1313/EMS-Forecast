namespace WetterdatenImporter.Utilities;

public static class PathResolver
{
    public static string ResolveFromCurrentDirectory(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new ArgumentException("Pfad darf nicht leer sein.", nameof(configuredPath));
        }

        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configuredPath));
    }
}
