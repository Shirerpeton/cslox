namespace cslox;

public interface ICallable {
    int Arity { get; }
    object? Call(Interpreter interpreter, List<object?> arguments);
}
public class Function: ICallable {
    public int Arity { get; init; }
    Func<Interpreter, List<object?>, object?> function;
    public Function(int arity, Func<Interpreter, List<object?>, object?> function) {
        this.Arity = arity;
        this.function = function;
    }
    public object? Call(Interpreter interpreter, List<object?> arguments) {
        return function(interpreter, arguments);
    }
}