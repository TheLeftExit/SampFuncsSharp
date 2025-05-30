## SampFuncsSharp

This sample uses .NET SDK's ability to [create native libraries](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/libraries) to write logic for GTA San Andreas mods in C#.

### How this works:
 - This sample implements a [SampFuncs](https://wiki.blast.hk/sampfuncs) plugin (since I'm writing a command/key binder for SAMP), but I think most of this should apply to a regular ASI plugin.
 - [SampFuncs.cs](ConsoleApp1/SampFuncs.cs) implements a blittable struct that holds several unmanaged function pointers, and an exports an entry point (`RegisterExports`) that allows C++ code to initialize those pointers. This struct also implements static methods that handle parameter mashalling and allow the rest of C# code to interact with these functions as regular static C# methods.
 - [Program.cs](ConsoleApp1/Program.cs) implements a simple function that takes a command parameter, splits it into words and sends each word as a chat message. The `Foo` method is a native export that handles parameter marshaling and runs the actual business logic (`FooCore`) in a separate thread (SampFuncs runs our code with `Thread.Sleep` in the main thread otherwise).
 - [plugin.cpp](https://github.com/TheLeftExit/SampFuncsSharp/blob/master/SF%20Plugin1/src/plugin.cpp) does the fun part:
   - It wraps relevant functions from SampFuncs' object model into simple static exports, so we can pass pointers to those methods to C# code.
   - It defines a struct with the same layout as the C# `SampFuncs` struct.
   - On startup, it loads our "managed" library from the `SAMPFUNCS` folder in the game directory, resolves `RegisterExports` and `Foo`, then immediately calls `RegisterExports` to initialize the `SampFuncs` C# struct, and stores the pointer to `Foo` for future use.
   - Finally, we it registers the `/test` command and forwards it to the C# `Foo` function (which splits input into words and sends them separately).

As someone who's terrified at the thought of writing non-trivial C++ code, this technology might be the only way for me to get into GTA modding. Hopefully it'll help those in a similar situation.

Note - of the functions made available in C#, I only tested `SendToChat`; the rest may or may not work. I also may or may not update the sample as I write the mod that uses all of them.

Another note - currently, the C++ project is configured to build directly to my game's `SAMPFUNCS` folder, and the C# project is missing a publishing configuration that's required to build it as a NativeAOT library, rather than a regular managed assembly. Take care to adjust these settings before you build the solution.
