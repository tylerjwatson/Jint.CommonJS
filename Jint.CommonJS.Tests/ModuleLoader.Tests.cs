
using System;
using System.IO;
using Jint;
using Jint.ModuleLoader;
using Xunit;

public class ModuleLoaderTests
{
    [Fact(DisplayName = "RunMain should throw an error if a module ID is not provided")]
    public void RunMainShouldThrowAnErrorIfAModuleIdIsNotProvided()
    {
        Assert.Throws<ArgumentException>(() => new Engine().CommonJS().RunMain(null));
    }

    [Fact(DisplayName = "RunMain should load an index file on disk")]
    public void RunMainShouldLoadAnIndexFileOnDisk()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());
        File.WriteAllText("index.js", @"
            exports.helloWorld = ""Hello World!"";
        ");

        var exports = new Engine().CommonJS().RunMain("./index");
        Assert.Equal("Hello World!", exports.AsObject().Get("helloWorld").AsString());
    }

    [Fact(DisplayName = "RunMain should load an index file via the './' construct")]
    public void RunMainShouldLoadAnIndexFileRelativelyOnDisk() {
        Directory.SetCurrentDirectory(Path.GetTempPath());
        File.WriteAllText("index.js", @"
            exports.helloWorld = ""Hello World 2"";
        ");

        var exports = new Engine().CommonJS().RunMain("./");
        Assert.Equal("Hello World 2", exports.AsObject().Get("helloWorld").AsString());
    }
}