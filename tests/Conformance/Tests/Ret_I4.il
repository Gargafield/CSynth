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
    .class public Ret_I4_Conformance {
        .method public static int32 Ret(int32) cil managed {
        .maxstack 1
            ldarg 0
            ret
        }

        .method public static int32 Main() {
        .locals init (int32)

            ldc.i4 0
            stloc 0

        TEST0:
            ldc.i4 0xAAAAAAAA
            call int32 ILConformance.Ret_I4_Conformance::Ret(int32)
            ldc.i4 0xAAAAAAAA
            ceq
            brfalse FAIL

            ldc.i4 1
            stloc 0

        TEST1:
            ldc.i4 0x55555555
            call int32 ILConformance.Ret_I4_Conformance::Ret(int32)
            ldc.i4 0x55555555
            ceq
            brfalse FAIL

            ldc.i4 2
            stloc 0

        TEST2:
            ldc.i4 0x7FFFFFFF
            call int32 ILConformance.Ret_I4_Conformance::Ret(int32)
            ldc.i4 0x7FFFFFFF
            ceq
            brfalse FAIL

            ldc.i4 3
            stloc 0

        TEST3:
            ldc.i4 0x00000001
            call int32 ILConformance.Ret_I4_Conformance::Ret(int32)
            ldc.i4 0x00000001
            ceq
            brfalse FAIL

            ldc.i4 4
            stloc 0

        TEST4:
            ldc.i4 0x00000000
            call int32 ILConformance.Ret_I4_Conformance::Ret(int32)
            ldc.i4 0x00000000
            ceq
            brfalse FAIL

            ldc.i4 5
            stloc 0

        TEST5:
            ldc.i4 0xFFFFFFFF
            call int32 ILConformance.Ret_I4_Conformance::Ret(int32)
            ldc.i4 0xFFFFFFFF
            ceq
            brfalse FAIL

            ldc.i4 6
            stloc 0

        TEST6:
            ldc.i4 0x80000000
            call int32 ILConformance.Ret_I4_Conformance::Ret(int32)
            ldc.i4 0x80000000
            ceq
            brfalse FAIL

        SUCCESS:
            ldstr "Ret_I4.il passed"
            call void [mscorlib]System.Console::WriteLine(string)
            ret

        FAIL:
            ldstr "Ret_I4.il failed at test number: {0}"
            ldloc 0
            call void [mscorlib]System.Console::WriteLine(string, object)
            
            ldc.i4 0
            ret
        }
    }
}
