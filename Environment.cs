namespace cslox;

public class Environment {
    Environment? enclosing;
    readonly Dictionary<string, object?> values = new Dictionary<string, object?>();
    public Environment(Environment? enclosing = null) {
        this.enclosing = enclosing;
    }
    public void Define(string name, object? value) {
        values.Add(name, value);
    }
    public object? Get(Token name) {
        if(values.TryGetValue(name.lexeme, out object? value)) {
            return value;
        }
        if(enclosing != null) {
            return enclosing.Get(name);
        }
        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }
    public void Assign(Token name, object? value) {
        if(values.TryGetValue(name.lexeme, out _)) {
            values[name.lexeme] = value;
            return;
        }
        if(enclosing != null) {
            enclosing.Assign(name, value);
            return;
        }
        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }
}