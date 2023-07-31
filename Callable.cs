namespace cslox;

public interface ICallable {
    int Arity { get; }
    object? Call(Interpreter interpreter, List<object?> arguments);
}
public class GlobalFunction: ICallable {
    public int Arity { get; init; }
    Func<Interpreter, List<object?>, object?> function;
    public GlobalFunction(int arity, Func<Interpreter, List<object?>, object?> function) {
        this.Arity = arity;
        this.function = function;
    }
    public object? Call(Interpreter interpreter, List<object?> arguments) {
        return function(interpreter, arguments);
    }
}
public class Function: ICallable {
    public int Arity { get; init; }
    Stmt.Function declaration;
    Environment closure;
    public Function(Stmt.Function declaration, Environment closure) {
        this.declaration = declaration;
        this.Arity = declaration.parameters.Count;
        this.closure = closure;
    }
    public object? Call(Interpreter interpreter, List<object?> arguments) {
        var environment = new Environment(closure);
        for(int i = 0; i < declaration.parameters.Count; i++) {
            environment.Define(declaration.parameters.ElementAt(i).lexeme, arguments.ElementAt(i), initialize: true);
        }
        try {
            interpreter.ExecuteBlock(declaration.body, environment);
        }
        catch(Return returnValue) {
            return returnValue.value;
        }
        return null;
    }
    public override string ToString() {
        return $"<fn {declaration.name.lexeme}>";
    }
}
public class Lambda: ICallable {
    public int Arity { get; init; }
    Expr.Lambda declaration;
    Environment closure;
    public Lambda(Expr.Lambda declaration, Environment closure) {
        this.declaration = declaration;
        this.Arity = declaration.parameters.Count;
        this.closure = closure;
    }
    public object? Call(Interpreter interpreter, List<object?> arguments) {
        var environment = new Environment(closure);
        for(int i = 0; i < declaration.parameters.Count; i++) {
            environment.Define(declaration.parameters.ElementAt(i).lexeme, arguments.ElementAt(i), initialize: true);
        }
        try {
            interpreter.ExecuteBlock(declaration.body, environment);
        }
        catch(Return returnValue) {
            return returnValue.value;
        }
        return null;
    }
    public override string ToString() {
        return "<fn>";
    }
}
