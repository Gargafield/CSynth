using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSynth.Compiler;

public static class CecilExtensions {
    
    public static VariableDefinition GetVariable(this Instruction instruction, MethodBody method) {
        if (instruction.Operand is VariableDefinition variable) {
            return variable;
        }

        switch (instruction.OpCode.Code) {
            case Code.Ldloc_0:
            case Code.Stloc_0:
                return method.Variables[0];
            case Code.Ldloc_1:
            case Code.Stloc_1:
                return method.Variables[1];
            case Code.Ldloc_2:
            case Code.Stloc_2:
                return method.Variables[2];
            case Code.Ldloc_3:
            case Code.Stloc_3:
                return method.Variables[3];
        }
        
        throw new NotSupportedException($"Unsupported instruction: {instruction}");
    }

    public static ParameterDefinition GetParameter(this Instruction instruction, MethodDefinition method) {
        if (instruction.Operand is ParameterDefinition parameter) {
            return parameter;
        }

        switch (instruction.OpCode.Code) {
            case Code.Ldarg_0:
            case Code.Ldarg_1:
            case Code.Ldarg_2:
            case Code.Ldarg_3:
                int index = (int)(instruction.OpCode.Code - Code.Ldarg_0);
                if (index < method.Parameters.Count) {
                    return method.Parameters[index];
                }
                if (method.IsConstructor && index == 0) {
                    return new ParameterDefinition("this", ParameterAttributes.None, method.DeclaringType);
                }

                throw new NotSupportedException($"Method {method.Name} does not have {index} parameters");
        }

        throw new NotSupportedException($"Unsupported instruction: {instruction}");
    }

    public static T GetValue<T>(this Instruction instruction) {
        if (instruction.Operand is T value) {
            return value;
        }

        throw new NotSupportedException($"Unsupported instruction: {instruction}");
    }

    public static int GetInt(this Instruction instruction) {
        if (instruction.Operand is int value) {
            return value;
        }
        if (instruction.Operand is sbyte shortValue) {
            return (int)shortValue;
        }

        switch (instruction.OpCode.Code) {
            case Code.Ldc_I4_0:
            case Code.Ldc_I4_1:
            case Code.Ldc_I4_2:
            case Code.Ldc_I4_3:
            case Code.Ldc_I4_4:
            case Code.Ldc_I4_5:
            case Code.Ldc_I4_6:
            case Code.Ldc_I4_7:
            case Code.Ldc_I4_8:
            case Code.Ldc_I4_M1:
                return (int)(instruction.OpCode.Code - Code.Ldc_I4_0);
        }

        throw new NotSupportedException($"Unsupported instruction: {instruction}");
    }
}