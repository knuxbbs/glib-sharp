#load Settings.cake

using System;
using P = System.IO.Path;

public class GAssembly
{
    private ICakeContext Cake;

    public string Name { get; private set; }
    public string Dir { get; private set; }
    public string GDir { get; private set; }
    public string Csproj { get; private set; }
    public string RawApi { get; private set; }
    public string Metadata { get; private set; }
    public string[] Deps { get; set; }
    public string ExtraArgs { get; set; }

    public GAssembly(string name)
    {
        Cake = Settings.Cake;
        Deps = new string[0];

        Name = name;
        Dir = P.Combine("Source", "Libs", name);
        GDir = P.Combine(Dir, "generated");

        var tempPath = P.Combine(Dir, name);
        Csproj = $"{tempPath}.csproj";
        RawApi = $"{Name}-api.xml";
        Metadata = $"{tempPath}.metadata";
    }

    public void Prepare()
    {
        if (!Cake.FileExists(RawApi))
        {
            return;
        }
        
        // Raw API file found, time to generate some stuff!!!
        var tempApi = P.Combine(GDir, $"{Name}-api.xml");

        // Fixup API file
        if (Cake.FileExists(Metadata))
        {
            var symFile = P.Combine(Dir, $"{Name}-symbols.xml");

            Cake.CopyFile(RawApi, tempApi);

            Cake.DotNetCoreExecute("BuildOutput/Tools/GapiFixup.dll",
                $"--metadata={Metadata} --api={tempApi}{(Cake.FileExists(symFile) ? $" --symbols={symFile}" : string.Empty)}"
            );
        }

        var extraArgs = $"{ExtraArgs} ";

        // Locate APIs to include
        foreach (var dep in Deps)
        {
            var ipath = P.Combine("Source", "Libs", dep, "generated", $"{dep}-api.xml");

            if (Cake.FileExists(ipath))
                extraArgs += $" --include={ipath} ";
        }

        Cake.CreateDirectory(GDir);

        // Generate code
        Cake.DotNetCoreExecute("BuildOutput/Tools/GapiCodegen.dll",
            $"--generate={RawApi} --assembly-name={Name} --outdir={GDir} --schema=BuildOutput/Tools/Gapi.xsd {extraArgs}"
        );
    }

    public void Clean()
    {
        if (Cake.DirectoryExists(GDir))
            Cake.DeleteDirectory(GDir, new DeleteDirectorySettings { Recursive = true, Force = true });
    }
}
