
using CSynth.AST;
using Mono.Cecil;

namespace CSynth.Analysis;

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
            statements.Add(new TypeDefinitionStatement(type));
            foreach (var method in type.Methods) {
                if (!method.HasBody) continue;

                var instructions = method.Body.Instructions;
                var compiled = Compiler.Compile(new MethodContext(method, Context));
                
                statements.Add(new MethodDefinitionStatement(method, compiled));
            }
        }

        if (Definition.EntryPoint != null) {
            var expression = new CallExpression(Definition.EntryPoint, new List<Expression>());
            statements.Add(new CallStatement(expression));
        }

        return statements;
    }
}