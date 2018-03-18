
using System.IO;
using Jint;
using Jint.ModuleLoader;
using Xunit;

public class ModuleTests {

    [Fact(DisplayName = "Modules should allow requiring of other modules from inside a module")]
    public void ItShouldAllowRequiringOfOtherModulesFromInsideAModule() {
        Directory.SetCurrentDirectory(Path.GetTempPath());

        File.WriteAllText("mod2.js", @"
            exports = {
                test: ""module 2"",
            }
        ");
        File.WriteAllText("index.js", @"
            require('./mod2');
        ");

        var exports = new Engine().CommonJS().RunMain("./");

        Assert.Equal("module 2", exports.AsString());
    }

    [Fact(DisplayName = "It should support overriding module.exports")]
    public void ItShouldSupportOverridingModuleExports() {
        Directory.SetCurrentDirectory(Path.GetTempPath());
        File.WriteAllText("index.js", @"
            module.exports = ""banana"";
        ");

        var exports = new Engine().CommonJS().RunMain("./");
        Assert.Equal("banana", exports.AsString());
    }

}