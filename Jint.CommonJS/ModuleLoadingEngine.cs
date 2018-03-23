
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jint.Native;
using Jint.Native.Object;

namespace Jint.CommonJS
{

    public class ModuleLoadingEngine
    {
        public delegate JsValue FileExtensionParser(string path, Module module);

        public Dictionary<string, Module> ModuleCache = new Dictionary<string, Module>();
        public Dictionary<string, FileExtensionParser> FileExtensionParsers = new Dictionary<string, FileExtensionParser>();

        public readonly Engine engine;
        public IModuleResolver Resolver { get; set; }

        public ModuleLoadingEngine(Engine e, IModuleResolver resolver = null)
        {
            this.engine = e;
            this.Resolver = resolver;

            FileExtensionParsers.Add("default", this.LoadJS);
            FileExtensionParsers.Add(".js", this.LoadJS);
            FileExtensionParsers.Add(".json", this.LoadJson);

            if (resolver == null)
            {
                this.Resolver = new CommonJSPathResolver(this.FileExtensionParsers.Keys);
            }
        }

        private JsValue LoadJS(string path, Module module)
        {
            var sourceCode = File.ReadAllText(path);
            module.exports = module.Compile(sourceCode, path);
            return module.exports;
        }

        private JsValue LoadJson(string path, Module module)
        {
            var sourceCode = File.ReadAllText(path);
            module.exports = engine.Json.Parse(JsValue.Undefined, new[] { JsValue.FromObject(this.engine, sourceCode) }).AsObject();
            return module.exports;
        }

        public JsValue RunMain(string mainModuleName)
        {
            if (string.IsNullOrWhiteSpace(mainModuleName))
            {
                throw new System.ArgumentException("A Main module path is required.", nameof(mainModuleName));
            }

            return this.Load(mainModuleName);
        }

        public JsValue Load(string moduleName, Module parent = null)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new System.ArgumentException("moduleName is required.", nameof(moduleName));
            }

            Module module = null;
            if (ModuleCache.ContainsKey(moduleName))
            {
                module = ModuleCache[moduleName];
                parent.children.Add(module);
                return module.exports;
            }

            module = new Module(this, moduleName, parent);

            return module.exports;
        }
    }
}