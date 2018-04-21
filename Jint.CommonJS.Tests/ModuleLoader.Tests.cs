
using System;
using System.IO;
using Jint;
using Jint.CommonJS;
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
        File.WriteAllText("index1.js", @"
            exports.helloWorld = ""Hello World!"";
        ");

        var exports = new Engine().CommonJS().RunMain("./index1");
        Assert.Equal("Hello World!", exports.AsObject().Get("helloWorld").AsString());
    }

    [Fact(DisplayName = "RunMain should load an index file via the './' construct")]
    public void RunMainShouldLoadAnIndexFileRelativelyOnDisk()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());
        File.WriteAllText("index2.js", @"
            exports.helloWorld = ""Hello World 2"";
        ");

        var exports = new Engine().CommonJS().RunMain("./index2");
        Assert.Equal("Hello World 2", exports.AsObject().Get("helloWorld").AsString());
    }

    [Fact(DisplayName = "It should require JSON")]
    public void ItShouldLoadJSON()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());

        File.WriteAllText("file.json", @"
            {
                ""testValue"": ""test""
            }
        ");

        var exports = new Engine().CommonJS().RunMain("./file.json");
        Assert.Equal("test", exports.AsObject().Get("testValue").AsString());
    }
}