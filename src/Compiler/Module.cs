using CSynth.AST;
using Mono.Cecil;

namespace CSynth.Compiler;

public class Module {
    public ModuleDefinition Definition { get; }
    public TranslationContext Context { get; }

    public Module(ModuleContext context) {
        Definition = context.Module;
        Context = context.TranslationContext;
    }

    public List<Statement> Compile() {

        var statements = new List<Statement>();

        foreach (var type in Definition.Types.Where(t => t.IsClass)) {
            var _type = new Type(new TypeContext(type, Context));
            statements.AddRange(_type.Compile());
        }

        if (Definition.EntryPoint != null) {
            var expression = new CallExpression(Definition.EntryPoint, new List<Expression>());
            statements.Add(new CallStatement(expression));
        }

        return statements;
    }
}