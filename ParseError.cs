namespace cslox;

public class ParseError: Exception {
    public readonly Token token;
    public readonly string message;
    public ParseError(Token token, string message): base() {
        this.token = token;
        this.message = message;
    }
}