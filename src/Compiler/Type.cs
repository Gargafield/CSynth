using CSynth.AST;
using Mono.Cecil;

namespace CSynth.Compiler;

public class Type {

    public TypeDefinition Definition { get; }
    public TranslationContext Context { get; }

    public Type(TypeContext context) {
        Definition = context.Type;
        Context = context.TranslationContext;
    }

    public List<Statement> Compile() {

        var statements = new List<Statement>();

        statements.Add(new TypeDefinitionStatement(Definition));
        
        foreach (var field in Definition.Fields) {
            var reference = new IndexExpression(new TypeExpression(Definition), new NumberExpression(field.RVA));
            statements.Add(new AssignmentStatement(reference, new ByteArrayExpression(field.InitialValue)));
        }

        foreach (var method in Definition.Methods) {
            if (!method.HasBody) continue;
            var compiled = Compiler.Compile(new MethodContext(method, Context));
            
            statements.Add(new MethodDefinitionStatement(method, compiled));
        }

        return statements;
    }
}