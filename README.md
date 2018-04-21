# Jint.CommonJS

What's a GitHub project without build badges? [![Build status](https://ci.appveyor.com/api/projects/status/eqxh6wt0gx7fdi04/branch/master?svg=true&retina=true)](https://ci.appveyor.com/project/tylerjwatson/jint-commonjs/branch/master)

Jint.CommonJS is an extremely simple CommonJS-compatible module loader for the [Jint .NET Javascript Engine](https://github.com/sebastienros/jint).  It's written in .NET core 2.0 and should be compatible with all .NET frameworks that Jint targets.  It allows you to require JavaScript modules from other modules in the Jint interpreter using the familiar `require` function we all know and love.

Every loaded module is wrapped in the following closure:

```
(function(module, exports, __dirname, require))
```

...and thus module bodies are not globally scoped.

* `module` points to the CLR module instance
* `exports` points to the module's public API
* `__dirname` is the directory that this module resides in
* `require` is a function which loads other modules relative to this module's directory

The library is MIT licensed.

## A note about Node.JS compatibilty

**You are not able to load npm packages with Jint.CommonJS.** Although the library loads modules in relatively the same format at NodeJS's [Module specification](https://nodejs.org/api/modules.html), there are some important distinctions.  The library does not support node_modules, or reading package.json files for modules.

## Features

* `require` another JavaScript module from a JavaScript file with `require('./module')`
* `require` JSON with `require('./file.json')`
* `require` modules from other modules
* Register internal modules with the `RegisterInternalModule` method
* A small but succinct unit test suite.

## Using the library
1.  Import the project reference via NuGet, or by cloning and building the project directly
1.  Import the `Jint.CommonJS` namespace in your code
1.  Use the `CommonJS()` extension method on `Jint.Engine` to enable CommonJS functionality

## Example

The following example runs a main module from the C# program's current directory.

```csharp
using Jint;
using Jint.CommonJS;

public static class Program
{
    public static Engine engine = new Engine();

    public static void Main(string[] args)
    {
        // Creates a new Jint instance and runs the myModule.js file in the program's
        // current working directory.
        Jint.Native.JsValue exports = engine.CommonJS().RunMain("./myModule");
    }
}
```

myModule.js
```js
exports.value = require('./myOtherModule');
```

## Internal Modules

Jint.CommonJS provides an API to register internal modules to any value which Jint supports.  It supports CLR type references, namespace references, and any `JsValue` instance, so you may require a CLR type into your module using the require mechanism.

Both static and instance members are supported when binding CLR types.

### Registering an mscorlib type as a module

```csharp
var e = new Engine();
var cjs = e
    .CommonJS()
    .RegisterInternalModule("console", typeof(Console))
    .RunMain("./")
```

index.js
```js
var console = require('console');
console.WriteLine('Test from System.Console!');
```

### Registering an internal module to a CLR class

```csharp
public class MyClass
{
    public void Method()
    {

    }
}

var cjs = e
    .CommonJS()
    .RegisterInternalModule("cls", typeof(MyClass))
    .RunMain("./")
```

index.js
```js
var cls = require('cls');
var instance = new cls();
cls.Method();
```