
using System.IO;
using Jint;
using Jint.CommonJS;
using Xunit;

public class ModuleTests
{

    [Fact(DisplayName = "Modules should allow requiring of other modules from inside a module")]
    public void ItShouldAllowRequiringOfOtherModulesFromInsideAModule()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());

        File.WriteAllText("mod3.js", @"
            module.exports = {
                test: ""module 3""
            }
        ");
        File.WriteAllText("index3.js", @"
            module.exports = require('./mod3');
        ");

        var exports = new Engine().CommonJS().RunMain("./index3");

        Assert.Equal("module 3", exports.AsObject().Get("test").AsString());
    }

    [Fact(DisplayName = "It should support overriding module.exports")]
    public void ItShouldSupportOverridingModuleExports()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());
        File.WriteAllText("index6.js", @"
            module.exports = ""banana"";
        ");

        var exports = new Engine().CommonJS().RunMain("./index6");
        Assert.Equal("banana", exports.AsString());
    }

    [Fact(DisplayName = "It should support recursively loading modules")]
    public void ItShouldSupportRecursivelyLoadingModules()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());

        File.WriteAllText("r_mod2.js", @"
            exports.func2 = function() {
                return 'func2';
            };

            exports.mod1 = require('./r_mod1');
        ");
        File.WriteAllText("r_mod1.js", @"
            module.exports = {
                func1: function() {
                    return 'func1';
                },
                mod2: require('./r_mod2'),
            }
        ");

        File.WriteAllText("r_main.js", @"
            module.exports = require('./r_mod1');
        ");

        var exports = new Engine().CommonJS().RunMain("./r_main");

        Assert.Equal("func1", exports.AsObject().Get("func1").Invoke(new Jint.Native.JsValue[] { }).AsString());
        Assert.Equal("func2", exports.AsObject().Get("mod2").AsObject().Get("func2").Invoke(new Jint.Native.JsValue[] { }).AsString());
    }
}