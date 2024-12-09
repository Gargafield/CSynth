
namespace CSynth.Core;

public interface ICompilableContext {
    public List<Statement> Compile(TranslationContext context);
}