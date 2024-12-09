using Mono.Cecil;

namespace CSynth.Core;

public class TypeContext : ICompilableContext {

    public TypeDefinition Definition { get; }

    public TypeContext(TypeDefinition definition) {
        Definition = definition;
    }

    public List<Statement> Compile(TranslationContext translationContext) {

        var statements = new List<Statement>();

        statements.Add(new TypeDefinitionStatement(Definition));
        
        foreach (var field in Definition.Fields) {
            var reference = new IndexExpression(new TypeExpression(Definition), new NumberExpression(field.RVA, TypeResolver.Int32Type));
            statements.Add(new AssignmentStatement(reference, new ByteArrayExpression(field.InitialValue)));
        }

        foreach (var method in Definition.Methods) {
            if (!method.HasBody) continue;
            var compiled = Compiler.Compile(method, translationContext);
            
            statements.Add(new MethodDefinitionStatement(method, compiled));
        }

        return statements;
    }
}