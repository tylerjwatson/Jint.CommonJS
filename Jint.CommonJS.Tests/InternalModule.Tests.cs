
using Jint;
using Xunit;
using Jint.CommonJS;
using Jint.Runtime.Interop;
using System.IO;
using System;

public class InternalModuleTests
{
    public class TestClass
    {
        public static string staticMethod()
        {
            return "test";
        }

        public string instanceMethod()
        {
            return "test instance";
        }
    }

    [Fact(DisplayName = "It supports interop to C# classes with type references")]
    public void ItSupportsTypeReferences()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());

        File.WriteAllText("ItSupportsTypeReferences.js", @"
            var testClass = require('test');
            var testInstance = new testClass();

            module.exports = {
                statics: testClass.staticMethod(),
                instances: testInstance.instanceMethod(),
            }
        ");

        var engine = new Engine();
        var exports = engine
            .CommonJS()
            .RegisterInternalModule("test", typeof(TestClass))
            .RunMain("./ItSupportsTypeReferences");

        Assert.Equal("test", exports.AsObject().Get("statics").AsString());
        Assert.Equal("test instance", exports.AsObject().Get("instances").AsString());
    }

    [Fact(DisplayName = "It supports delegates")]
    public void ItSupportsDelegates()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());

        File.WriteAllText("ItSupportsDelegates.js", @"
            module.exports = require('func')();
        ");
        var engine = new Engine();
        var exports = engine
            .CommonJS()
            .RegisterInternalModule("func", new Func<string>(() => "test delegate value"))
            .RunMain("./ItSupportsDelegates");
         
         Assert.Equal("test delegate value", exports.AsString());
    }
}