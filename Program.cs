namespace cslox;
static class Program {
    static IErrorReporter errorReporter = new ErrorReporter();
    static void Main(string[] args) {
        if(args.Length > 1) {
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
        if(errorReporter.HadError) {
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
            if(errorReporter.HadError) {
                errorReporter.HadError = false;
            }
        }
    }

    static void Run(string source) {
        var scanner = new Scanner(errorReporter, source);
        List<Token> tokens = scanner.ScanTokens();
        if(errorReporter.HadError) {
            return;
        }
        Parser parser = new Parser(errorReporter, tokens);
        Expr.Expr? tree = parser.Parse();
        if(errorReporter.HadError) {
            return;
        }
        if(tree == null) {
            errorReporter.ReportError(0, "Empty AST.");
            return;
        }
        Console.WriteLine(new AstRPNPrinter().Print(tree));
    }


}