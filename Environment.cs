namespace cslox;

public record struct VariableValue {
    public required object? value;
    public required bool initialized;
}

public class Environment {
    Environment? enclosing;
    readonly Dictionary<string, VariableValue> values = new Dictionary<string, VariableValue>();
    public Environment(Environment? enclosing = null) {
        this.enclosing = enclosing;
    }
    public void Define(string name, object? value, bool initialize = true) {
        var variableValue = new VariableValue { initialized = initialize, value = value };
        values.Add(name, variableValue);
    }
    public object? Get(Token name) {
        if(values.TryGetValue(name.lexeme, out VariableValue value)) {
            if(!value.initialized) {
                throw new RuntimeError(name, $"Unassigned variable {name.lexeme} used.");
            }
            return value.value;
        }
        if(enclosing != null) {
            return enclosing.Get(name);
        }
        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }
    public void Assign(Token name, object? value) {
        if(values.TryGetValue(name.lexeme, out _)) {
            var variableValue = new VariableValue { initialized = true, value = value };
            values[name.lexeme] = variableValue;
            return;
        }
        if(enclosing != null) {
            enclosing.Assign(name, value);
            return;
        }
        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }
}
