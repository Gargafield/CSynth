
.assembly extern mscorlib {}

.assembly Examples
{
    .ver 1:0:0:0
}

.class public Examples.Examples
{
    .method public static void Example1() cil managed
    {
        .maxstack 2
        ldc.i4.0
        stloc.0
        br.s IL_0012
    IL_0004:
        ldstr "Hello, World!"
        call void [mscorlib]System.Console::WriteLine(string)
        ldloc.0
        ldc.i4.1
        add
        stloc.0
    IL_0012:
        ldloc.0
        ldc.i4.5
        blt.s IL_0004
        ret
    }

    .method public static void Example2() cil managed
    {
        .maxstack 2
        ldc.i4.0
        stloc.0
        br.s IL_0016
    IL_0004:
        ldstr "Hello, World!"
        call void [mscorlib]System.Console::WriteLine(string)
        ldloc.0
        ldc.i4.1
        add
        stloc.0
    IL_0016:
        ldloc.0
        ldc.i4.5
        blt.s IL_0004
        ret
    }
}