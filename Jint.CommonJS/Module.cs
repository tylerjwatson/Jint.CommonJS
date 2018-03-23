
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;

namespace Jint.CommonJS
{

    public class Module
    {
        /// <summary>
        /// This module's children
        /// </summary>
        public List<Module> children = new List<Module>();

        public readonly string id;

        protected Module parentModule;

        protected ModuleLoadingEngine engine;

        /// <summary>
        /// Determines if this module is the main module.
        /// </summary>
        public bool isMainModule => this.parentModule == null;

        /// <summary>
        /// Contains the module's public API.
        /// </summary>
        public JsValue exports;

        public readonly string filePath;

        /// <summary>
        /// Creates a new Module instaznce with the specified module id. The module is resolved to a file on disk
        /// according to the CommonJS specification.
        /// </summary>
        internal Module(ModuleLoadingEngine e, string moduleId, Module parent = null)
        {
            if (e == null)
            {
                throw new System.ArgumentNullException(nameof(e));
            }

            this.engine = e;

            if (string.IsNullOrEmpty(moduleId))
            {
                throw new System.ArgumentException("A moduleId is required.", nameof(moduleId));
            }

            this.id = moduleId;
            this.filePath = e.Resolver.ResolvePath(this.id, this);
            this.parentModule = parent;

            if (parent != null)
            {
                parent.children.Add(this);
            }

            this.exports = engine.engine.Object.Construct(new JsValue[] {});

            string extension = Path.GetExtension(this.filePath);
            var loader = this.engine.FileExtensionParsers[extension] ?? this.engine.FileExtensionParsers["default"];

            e.ModuleCache.Add(this.id, this);

            loader(this.filePath, this);
        }
        
        protected JsValue Require(string moduleId) {
            return engine.Load(moduleId, this);
        }

        public JsValue Compile(string sourceCode, string filePath)
        {
            engine.engine.Execute($@"
                ;(function (module, exports, __dirname, require) {{
                    {sourceCode}
                }})
            ").GetCompletionValue().As<FunctionInstance>().Call(
                JsValue.FromObject(this.engine.engine, this),
                new JsValue[] {
                    JsValue.FromObject(this.engine.engine, this),
                    this.exports,
                    Path.GetDirectoryName(filePath),
                    new ClrFunctionInstance(this.engine.engine, (thisObj, arguments) => Require(arguments.At(0).AsString()))
                    //  new DelegateWrapper(engine.engine, new Func<string, JsValue>(this.Require)),
                    }
                );

                return exports;
        }
    }

}