using Mono.Cecil;

namespace CSynth.Core;

public class ModuleContext : ICompilableContext {
    public ModuleDefinition Module { get; }

    public ModuleContext(ModuleDefinition definition) {
        Module = definition;
    }

    public List<Statement> Compile(TranslationContext context) {

        var statements = new List<Statement>();

        foreach (var type in Module.Types.Where(t => t.IsClass)) {
            var _type = new TypeContext(type);
            statements.AddRange(_type.Compile(context));
        }

        if (Module.EntryPoint != null) {
            var function = new MethodExpression(Module.EntryPoint);
            var expression = new CallExpression(function, new List<Expression>());
            statements.Add(new CallStatement(expression));
        }

        return statements;
    }
}