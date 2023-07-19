namespace cslox;

public interface IErrorReporter {
    bool HadError { get; set; }
    void ReportError(int line, string message);
}
public class ErrorReporter: IErrorReporter {
    public bool HadError { get; set; } = false;
    public void ReportError(int line, string message) {
        Print(line, "", message);
        HadError = true;
    }

    void Print(int line, string where, string message) {
        Console.WriteLine($"[line {line} Error {where}: {message}]");
    }
}