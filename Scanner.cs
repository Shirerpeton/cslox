namespace cslox;

public class Scanner {
    readonly string source;
    public Scanner(string source) {
        this.source = source;
    }
    public List<Token> ScanTokens() {
        return new List<Token>();
    }
}