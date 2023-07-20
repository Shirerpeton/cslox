namespace cslox;

public class Scanner {
    IErrorReporter errorReporter;
    readonly string source;
    readonly List<Token> tokens = new List<Token>(); 
    int start = 0;
    int current = 0;
    int line = 1;
    bool IsAtEnd => current >= source.Length;
    public Scanner(IErrorReporter errorReporter, string source) {
        this.errorReporter = errorReporter;
        this.source = source;
    }
    public List<Token> ScanTokens() {
        while(!IsAtEnd) {
            start = current;
            ScanToken();
        }
        tokens.Add(new Token(TokenType.EOF, "", null, line));
        return tokens;
    }
    void ScanToken() {
        char c = Advance();
        switch(c) {
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case '{':
                AddToken(TokenType.LeftBrace);
                break;
            case '}':
                AddToken(TokenType.RightBrace);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '-':
                AddToken(TokenType.Minus);
                break;
            case '+':
                AddToken(TokenType.Plus);
                break;
            case ';':
                AddToken(TokenType.Semicolon);
                break;
            case '*':
                AddToken(TokenType.Star);
                break;

            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;

            case '/':
                if(Match('/')) {
                    while(Peek() != '\n' && !IsAtEnd) {
                        Advance();
                    }
                } else {
                    AddToken(TokenType.Slash);
                }
                break;

            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                line++;
                break;

            case '"':
                String();
                break;

            default:
                errorReporter.ReportError(line, message: "Unexpected character");
                break;
        }
    }
    char Advance() {
        return source.ElementAt(current++);
    }
    bool Match(char expexted) {
        if(IsAtEnd) {
            return false;
        }
        if(source.ElementAt(current) != expexted) {
            return false;
        }
        current++;
        return true;
    }
    char Peek() {
        if(IsAtEnd) {
            return '\0';
        }
        return source.ElementAt(current);
    }
    void String() {
        while(Peek() != '"' && !IsAtEnd) {
            if(Peek() == '\n') {
                line++;
            }
            Advance();
        }

        if(IsAtEnd) {
            errorReporter.ReportError(line, "Unterminated string.");
            return;
        }

        Advance();

        string value = source.Substring(start + 1, (current - start) - 2);
        AddToken(TokenType.String, value);
    }
    bool IsDigit(char c) {
        return c >= '0' && c <= '9'; 
    }
    void number() {
        while(IsDigit(Peek())) {
            Advance();
        }
        if(Peek() == '.' && IsDigit(PeekNext())) {
            Advance();
            while(IsDigit(Peek())) {
                Advance();
            }
        }
        AddToken(TokenType.Number, double.Parse(source.Substring(start, current - start)));
    }
    char PeekNext() {
        if(current + 1 >= source.Length) {
            return '\0';
        }
        return source.ElementAt(current + 1);
    }
    void AddToken(TokenType type) {
        AddToken(type, literal: null);
    }
    void AddToken(TokenType type, object? literal) {
        string text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }
}