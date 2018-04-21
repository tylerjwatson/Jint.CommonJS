
using Jint;
using Xunit;
using Jint.CommonJS;
using Jint.Runtime.Interop;
using System.IO;
using System;
using Jint.Native;

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

    [Fact(DisplayName = "It supports Object Instances")]
    public void ItSupportsObjectInstances()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());

        File.WriteAllText("ItSupportsObjectInstances.js", @"
            module.exports = require('objectInstance');
        ");

        var testObject = new
        {
            test = "test value"
        };

        var engine = new Engine();
        var exports = engine
            .CommonJS()
            .RegisterInternalModule("objectInstance", testObject)
            .RunMain("./ItSupportsObjectInstances");

        Assert.Equal("test value", exports.AsObject().Get("test").AsString());
    }

    [Fact(DisplayName = "It raises the ModuleRequested event when being asked for an internal module not in the cache")]
    public void ItRaisesTheModuleRequestedEvent()
    {
        var engine = new Engine().CommonJS();
        var testModuleName = "TestModule";

        Directory.SetCurrentDirectory(Path.GetTempPath());

        File.WriteAllText("ModuleRequestedEvent.js", $@"
            module.exports = require('{testModuleName}');
        ");

        var eventData = Assert.RaisesAny<ModuleRequestedEventArgs>(e => engine.ModuleRequested += e, e => engine.ModuleRequested -= e, () => {
            // An exception is expected here; the RaisesAny method does not assign the exports a value.
            Assert.Throws<FileNotFoundException>(() => engine.RunMain("./ModuleRequestedEvent"));
        });

        Assert.Equal(testModuleName, eventData.Arguments.ModuleId);
    }

    [Fact]
    public void ItLoadsModulesFromTheModuleRequestedEventHandler()
    {
        Directory.SetCurrentDirectory(Path.GetTempPath());

        var engine = new Engine().CommonJS();
        var testModuleName = "TestModule";

        File.WriteAllText("ModuleRequestedEvent2.js", $@"
            module.exports = require('{testModuleName}');
        ");

        engine.ModuleRequested += (sender, args) => {
            if (args.ModuleId == testModuleName)
            {
                args.Exports = JsValue.FromObject(engine.engine, "test value");
            }
        };

        var exports = engine.RunMain("./ModuleRequestedEvent2");

        Assert.Equal("test value", exports.AsString());
    }


}