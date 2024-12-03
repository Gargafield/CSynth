// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modifications made by Gargafield and licenses these modifications to you under the MIT license.

.assembly ILConformance
{
    .ver 1:0:0:0
}

.assembly extern mscorlib
{
    .ver 4:0:0:0
}

.namespace ILConformance
{
    .class public Add_I4_Conformance {
        .method public static bool Add(int32 a, int32 b, int32 result) cil managed
        {
            ldarg.0
            ldarg.1
            add
            ldarg.2
            ceq
            ret
        }

        .method public static int32 Main() {
            .entrypoint
            .locals init (int32)

            ldc.i4 1
            stloc 0

        TEST1:
            ldc.i4 0x80000000
            ldc.i4 0x80000000
            ldc.i4 0x00000000
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL
            
            ldc.i4 2
            stloc 0

        TEST2:
            ldc.i4 0x80000000
            ldc.i4 0xFFFFFFFF
            ldc.i4 0x7FFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 3
            stloc 0

        TEST3:
            ldc.i4 0x80000000
            ldc.i4 0x00000000
            ldc.i4 0x80000000
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 4
            stloc 0

        TEST4:
            ldc.i4 0x80000000
            ldc.i4 0x00000001
            ldc.i4 0x80000001
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 5
            stloc 0

        TEST5:
            ldc.i4 0x80000000
            ldc.i4 0x7FFFFFFF
            ldc.i4 0xFFFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 6
            stloc 0

        TEST6:
            ldc.i4 0x80000000
            ldc.i4 0x55555555
            ldc.i4 0xD5555555
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 7
            stloc 0

        TEST7:
            ldc.i4 0x80000000
            ldc.i4 0xAAAAAAAA
            ldc.i4 0x2AAAAAAA
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 8
            stloc 0

        TEST8:
            ldc.i4 0xFFFFFFFF
            ldc.i4 0x80000000
            ldc.i4 0x7FFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 9
            stloc 0

        TEST9:
            ldc.i4 0xFFFFFFFF
            ldc.i4 0xFFFFFFFF
            ldc.i4 0xFFFFFFFE
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 10
            stloc 0

        TEST10:
            ldc.i4 0xFFFFFFFF
            ldc.i4 0x00000000
            ldc.i4 0xFFFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 11
            stloc 0

        TEST11:
            ldc.i4 0xFFFFFFFF
            ldc.i4 0x00000001
            ldc.i4 0x00000000
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 12
            stloc 0

        TEST12:
            ldc.i4 0xFFFFFFFF
            ldc.i4 0x7FFFFFFF
            ldc.i4 0x7FFFFFFE
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 13
            stloc 0

        TEST13:
            ldc.i4 0xFFFFFFFF
            ldc.i4 0x55555555
            ldc.i4 0x55555554
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 14
            stloc 0

        TEST14:
            ldc.i4 0xFFFFFFFF
            ldc.i4 0xAAAAAAAA
            ldc.i4 0xAAAAAAA9
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 15
            stloc 0

        TEST15:
            ldc.i4 0x00000000
            ldc.i4 0x80000000
            ldc.i4 0x80000000
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 16
            stloc 0

        TEST16:
            ldc.i4 0x00000000
            ldc.i4 0xFFFFFFFF
            ldc.i4 0xFFFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 17
            stloc 0

        TEST17:
            ldc.i4 0x00000000
            ldc.i4 0x00000000
            ldc.i4 0x00000000
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 18
            stloc 0

        TEST18:
            ldc.i4 0x00000000
            ldc.i4 0x00000001
            ldc.i4 0x00000001
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 19
            stloc 0

        TEST19:
            ldc.i4 0x00000000
            ldc.i4 0x7FFFFFFF
            ldc.i4 0x7FFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 20
            stloc 0

        TEST20:
            ldc.i4 0x00000000
            ldc.i4 0x55555555
            ldc.i4 0x55555555
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 21
            stloc 0

        TEST21:
            ldc.i4 0x00000000
            ldc.i4 0xAAAAAAAA
            ldc.i4 0xAAAAAAAA
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 22
            stloc 0

        TEST22:
            ldc.i4 0x00000001
            ldc.i4 0x80000000
            ldc.i4 0x80000001
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 23
            stloc 0

        TEST23:
            ldc.i4 0x00000001
            ldc.i4 0xFFFFFFFF
            ldc.i4 0x00000000
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 24
            stloc 0

        TEST24:
            ldc.i4 0x00000001
            ldc.i4 0x00000000
            ldc.i4 0x00000001
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 25
            stloc 0

        TEST25:
            ldc.i4 0x00000001
            ldc.i4 0x00000001
            ldc.i4 0x00000002
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 26
            stloc 0

        TEST26:
            ldc.i4 0x00000001
            ldc.i4 0x7FFFFFFF
            ldc.i4 0x80000000
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 27
            stloc 0

        TEST27:
            ldc.i4 0x00000001
            ldc.i4 0x55555555
            ldc.i4 0x55555556
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 28
            stloc 0

        TEST28:
            ldc.i4 0x00000001
            ldc.i4 0xAAAAAAAA
            ldc.i4 0xAAAAAAAB
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 29
            stloc 0

        TEST29:
            ldc.i4 0x7FFFFFFF
            ldc.i4 0x80000000
            ldc.i4 0xFFFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 30
            stloc 0

        TEST30:
            ldc.i4 0x7FFFFFFF
            ldc.i4 0xFFFFFFFF
            ldc.i4 0x7FFFFFFE
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 31
            stloc 0

        TEST31:
            ldc.i4 0x7FFFFFFF
            ldc.i4 0x00000000
            ldc.i4 0x7FFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 32
            stloc 0

        TEST32:
            ldc.i4 0x7FFFFFFF
            ldc.i4 0x00000001
            ldc.i4 0x80000000
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 33
            stloc 0

        TEST33:
            ldc.i4 0x7FFFFFFF
            ldc.i4 0x7FFFFFFF
            ldc.i4 0xFFFFFFFE
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 34
            stloc 0

        TEST34:
            ldc.i4 0x7FFFFFFF
            ldc.i4 0x55555555
            ldc.i4 0xD5555554
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 35
            stloc 0

        TEST35:
            ldc.i4 0x7FFFFFFF
            ldc.i4 0xAAAAAAAA
            ldc.i4 0x2AAAAAA9
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 36
            stloc 0

        TEST36:
            ldc.i4 0x55555555
            ldc.i4 0x80000000
            ldc.i4 0xD5555555
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 37
            stloc 0

        TEST37:
            ldc.i4 0x55555555
            ldc.i4 0xFFFFFFFF
            ldc.i4 0x55555554
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 38
            stloc 0

        TEST38:
            ldc.i4 0x55555555
            ldc.i4 0x00000000
            ldc.i4 0x55555555
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 39
            stloc 0

        TEST39:
            ldc.i4 0x55555555
            ldc.i4 0x00000001
            ldc.i4 0x55555556
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 40
            stloc 0

        TEST40:
            ldc.i4 0x55555555
            ldc.i4 0x7FFFFFFF
            ldc.i4 0xD5555554
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 41
            stloc 0

        TEST41:
            ldc.i4 0x55555555
            ldc.i4 0x55555555
            ldc.i4 0xAAAAAAAA
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 42
            stloc 0

        TEST42:
            ldc.i4 0x55555555
            ldc.i4 0xAAAAAAAA
            ldc.i4 0xFFFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 43
            stloc 0

        TEST43:
            ldc.i4 0xAAAAAAAA
            ldc.i4 0x80000000
            ldc.i4 0x2AAAAAAA
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 44
            stloc 0

        TEST44:
            ldc.i4 0xAAAAAAAA
            ldc.i4 0xFFFFFFFF
            ldc.i4 0xAAAAAAA9
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 45
            stloc 0

        TEST45:
            ldc.i4 0xAAAAAAAA
            ldc.i4 0x00000000
            ldc.i4 0xAAAAAAAA
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 46
            stloc 0

        TEST46:
            ldc.i4 0xAAAAAAAA
            ldc.i4 0x00000001
            ldc.i4 0xAAAAAAAB
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 47
            stloc 0

        TEST47:
            ldc.i4 0xAAAAAAAA
            ldc.i4 0x7FFFFFFF
            ldc.i4 0x2AAAAAA9
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 48
            stloc 0

        TEST48:
            ldc.i4 0xAAAAAAAA
            ldc.i4 0x55555555
            ldc.i4 0xFFFFFFFF
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 49
            stloc 0

        TEST49:
            ldc.i4 0xAAAAAAAA
            ldc.i4 0xAAAAAAAA
            ldc.i4 0x55555554
            call bool ILConformance.Add_I4_Conformance::Add(int32,int32,int32)
            brfalse FAIL

            ldc.i4 50
            stloc 0

        SUCCESS:
            ldstr "Add_I4.il passed"
            call void [mscorlib]System.Console::WriteLine(string)
            ret

        FAIL:
            ldstr "Add_I4.il failed at test number: {0}"
            ldloc 0
            call void [mscorlib]System.Console::WriteLine(string, object)
            
            ldc.i4 0
            ret
        }
    }
}