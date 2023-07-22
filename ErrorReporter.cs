namespace cslox;

public interface IErrorReporter {
    bool HadError { get; set; }
    void ReportError(int line, string message);
    void ReportError(Token token, string message);
}
public class ErrorReporter: IErrorReporter {
    public bool HadError { get; set; } = false;
    public void ReportError(int line, string message) {
        Print(line, "", message);
        HadError = true;
    }
    public void ReportError(Token token, string message) {
        if(token.type == TokenType.EOF) {
            Print(token.line, " at end", message);
        } else {
            Print(token.line, $" at '{token.lexeme}'", message);
        }
        HadError = true;
    }

    void Print(int line, string where, string message) {
        Console.WriteLine($"[line {line} Error {where}: {message}]");
    }
}