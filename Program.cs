namespace cslox;
static class Program {
    static void Main(string[] args){
        if(args.Length > 0) {
            Console.WriteLine("Usage: sclox [script]");
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
        }
        string content = File.ReadAllText(path);
        Run(content);
    }

    static void RunPrompt() {
        while(true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if(string.IsNullOrEmpty(line)) {
                break;
            }
            Run(line);
        }
    }

    static void Run(string source) {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        foreach(Token token in tokens) {
            Console.WriteLine(token);
        }
    }
}