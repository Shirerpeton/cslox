namespace cslox;
static class Program {
    static IErrorReporter errorReporter = new ErrorReporter();
    static void Main(string[] args) {
        if(args.Length > 1) {
            Console.WriteLine("Usage: sclox [script]");
            System.Environment.ExitCode = 64;
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
            System.Environment.ExitCode = 64;
            return;
        }
        string content = File.ReadAllText(path);
        Run(content);
        if(errorReporter.HadError) {
            System.Environment.ExitCode = 65;
        }
        if(errorReporter.HadRuntimeError) {
            System.Environment.ExitCode = 70;
        }
    }

    static void RunPrompt() {
        while(true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if(string.IsNullOrEmpty(line)) {
                break;
            }
            Run(line, isRepl: true);
            if(errorReporter.HadError) {
                errorReporter.HadError = false;
            }
        }
    }

    static void Run(string source, bool isRepl = false) {
        var scanner = new Scanner(errorReporter, source);
        List<Token> tokens = scanner.ScanTokens();
        if(errorReporter.HadError) {
            return;
        }
        Parser parser = new Parser(errorReporter, tokens, isRepl);
        List<Stmt.Stmt> statements = parser.Parse();
        if(errorReporter.HadError) {
            return;
        }
        //var printer = new AstPrinter();
        //printer.Print(statements);
        Interpreter interpreter = new Interpreter(errorReporter);
        interpreter.Interpret(statements);
    }
}
