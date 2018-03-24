
using System.Collections.Generic;
using Jint.Native;
using Jint.Native.Function;

namespace Jint.CommonJS
{
    public class InternalModule : IModule
    {
        public string Id { get; set; }

        public List<IModule> Children => new List<IModule>();

        public JsValue Exports => exports;

        protected JsValue exports;

        public InternalModule(string id, JsValue constructor)
        {
            this.Id = id;
            exports = constructor;
        }
    }
}