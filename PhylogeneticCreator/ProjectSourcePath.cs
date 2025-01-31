﻿using System.Runtime.CompilerServices;

internal static class ProjectSourcePath
{
    private const  string  myRelativePath = nameof(ProjectSourcePath) + ".cs";
    private static string? lazyValue;
    public  static string  Value => lazyValue ??= calculatePath();

    private static string calculatePath()
    {
        string pathName = GetSourceFilePathName();
        Assert( pathName.EndsWith( myRelativePath, StringComparison.Ordinal ) );
        return pathName.Substring( 0, pathName.Length - myRelativePath.Length );
    }
    
    private static string GetSourceFilePathName( [CallerFilePath] string? callerFilePath = null ) //
        => callerFilePath ?? "";

    private static void Assert(bool condition)
    {
        if (!condition)
            throw new Exception("Assertion failed");
    }
}