using System;
using Mono.Cecil;

namespace CSynth.AST;

public class MethodContext {
    public MethodDefinition Method { get; }
    public TranslationContext TranslationContext { get; set; }

    public MethodContext(MethodDefinition method, TranslationContext context) {
        Method = method;
        TranslationContext = context;
    }
}

public class TypeContext {
    public TypeDefinition Type { get; }
    public TranslationContext TranslationContext { get; set; }

    public TypeContext(TypeDefinition type, TranslationContext context) {
        Type = type;
        TranslationContext = context;
    }
}

public class ModuleContext {
    public ModuleDefinition Module { get; }
    public TranslationContext TranslationContext { get; set; }

    public ModuleContext(ModuleDefinition module, TranslationContext context) {
        Module = module;
        TranslationContext = context;
    }
}

public class TranslationContext {
    public bool Debug { get; set; }
}