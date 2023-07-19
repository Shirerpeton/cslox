namespace cslox;
static class Program {
    static bool hadError = false;
    static void Main(string[] args){
        if(args.Length > 0) {
            Console.WriteLine("Usage: sclox [script]");
            Environment.ExitCode = 64;
            return;
        } else if(args.Length == 1) {
            RunFile(args[0]);
        } else {
            RunPrompt();
        }
    }

    static void RunFile(string path) {
        if(!File.Exists(path)) {
            Console.WriteLine("File doesn't exist!");
            Environment.ExitCode = 64;
            return;
        }
        string content = File.ReadAllText(path);
        Run(content);
        if(hadError) {
            Environment.ExitCode = 65;
        }
    }

    static void RunPrompt() {
        while(true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if(string.IsNullOrEmpty(line)) {
                break;
            }
            Run(line);
            if(hadError) {
                hadError = false;
            }
        }
    }

    static void Run(string source) {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        foreach(Token token in tokens) {
            Console.WriteLine(token);
        }
    }

    static void Error(int line, string message) {
        Report(line, "", message);
        hadError = true;
    }

    static void Report(int line, string where, string message) {
        Console.WriteLine($"[line {line} Error {where}: {message}");
    }
}