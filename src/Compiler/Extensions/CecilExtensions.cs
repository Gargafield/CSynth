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
                if (method.HasThis || method.IsConstructor) {
                    index--;
                }

                if (index < 0) {
                    return new ParameterDefinition("this", ParameterAttributes.None, method.DeclaringType);
                }

                if (index < method.Parameters.Count) {
                    return method.Parameters[index];
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

    public static int GetSize(this TypeReference type) {
        if (type.IsPrimitive) {
            switch (type.MetadataType) {
                case MetadataType.Boolean:
                case MetadataType.Byte:
                case MetadataType.SByte:
                    return 1;
                case MetadataType.Int16:
                case MetadataType.UInt16:
                    return 2;
                case MetadataType.Int32:
                case MetadataType.UInt32:
                case MetadataType.Single:
                    return 4;
                case MetadataType.Int64:
                case MetadataType.UInt64:
                case MetadataType.Double:
                    return 8;
            }
        }

        if (type.IsPointer || type.IsFunctionPointer) {
            return 4;
        }

        if (type.IsValueType) {
            int size = 0;
            foreach (var field in type.Resolve().Fields) {
                if (field.IsStatic)
                    continue;
                if (field.FieldType == type)
                    continue; // Avoid infinite recursion
                size += GetSize(field.FieldType);
            }

            return Math.Max(1, size);
        }

        if (type.IsGenericParameter) {
            // TODO: Is this even remotely correct?
            return 4;
        }

        throw new NotSupportedException($"Unsupported type: {type}");
    }
}