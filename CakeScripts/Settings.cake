#load GAssembly.cake

using System.Collections.Generic;

class Settings
{
    public static ICakeContext Cake { get; set; }
    public static string Version { get; set; }
    public static string BuildTarget { get; set; }
    public static List<GAssembly> AssemblyList { get; set; }
    
    public static void Init()
    {
        AssemblyList = new List<GAssembly>()
        {
            new GAssembly("GLibSharp")
        };
    }
}