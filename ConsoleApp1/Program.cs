using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static unsafe class MyClass
{
    [UnmanagedCallersOnly(EntryPoint = "Foo", CallConvs = [typeof(CallConvStdcall)])]
    public static void Foo(byte* input)
    {
        // Marshalling + safety from string deallocation.
        var inputString = Marshal.PtrToStringUTF8((nint)input) ?? string.Empty;
        // Separate thread to allow Thread.Sleep without blocking the game.
        // SampFuncs seems to marshal everything to the game thread anyway, and Lua scripts already run in background threads by default.
        new Thread(() => FooCore(inputString)).Start();
    }

    private static void FooCore(string input)
    {
        foreach(var word in input.Split(' '))
        {
            if (word.Length > 0)
            {
                SampFuncs.SendToChat(word);
                Thread.Sleep(500);
            }
        }
    }
}
