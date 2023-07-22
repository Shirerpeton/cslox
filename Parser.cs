namespace cslox;

public class Parser {
    readonly List<Token> tokens;
    readonly IErrorReporter errorReporter;
    int current = 0;
    bool IsAtEnd => Peek.type == TokenType.EOF;
    Token Peek => tokens.ElementAt(current);
    Token Previous => tokens.ElementAt(current - 1);

    public Parser(IErrorReporter errorReporter, List<Token> tokens) {
        this.errorReporter = errorReporter;
        this.tokens = tokens;
    }
    public Expr.Expr? Parse() {
        try {
            return Expression();
        } catch(ParseError) {
            return null;
        }
    }
    Expr.Expr Expression() {
        return PartialExpr();
    }
    Expr.Expr PartialExpr() {
        Expr.Expr expr = Equality();
        while(Match(new TokenType[] { TokenType.Comma })) {
            Token opr = Previous;
            Expr.Expr right = Equality();
            expr = new Expr.Binary(expr, opr, right);
        }
        return expr;
    }
    Expr.Expr Equality() {
        Expr.Expr expr = Comparison();
        while(Match(new TokenType[] { TokenType.BangEqual, TokenType.EqualEqual })) {
            Token opr = Previous;
            Expr.Expr right = Comparison();
            expr = new Expr.Binary(expr, opr, right);
        }
        return expr;
    }
    Expr.Expr Comparison() {
        Expr.Expr expr = Term();
        while(Match(new TokenType[] { TokenType.Less, TokenType.LessEqual, TokenType.Greater, TokenType.GreaterEqual })) {
            Token opr = Previous;
            Expr.Expr right = Term();
            expr = new Expr.Binary(expr, opr, right);
        }
        return expr;
    }
    Expr.Expr Term() {
        Expr.Expr expr = Factor();
        while(Match(new TokenType[] { TokenType.Minus, TokenType.Plus })) {
            Token opr = Previous;
            Expr.Expr right = Factor();
            expr = new Expr.Binary(expr, opr, right);
        }
        return expr;
    }
    Expr.Expr Factor() {
        Expr.Expr expr = Unary();
        while(Match(new TokenType[] { TokenType.Slash, TokenType.Star })) {
            Token opr = Previous;
            Expr.Expr right = Unary();
            expr = new Expr.Binary(expr, opr, right);
        }
        return expr;
    }
    Expr.Expr Unary() {
        if(Match(new TokenType[] { TokenType.Bang, TokenType.Minus })) {
            Token opr = Previous;
            Expr.Expr right = Unary();
            return new Expr.Unary(opr, right);
        }
        return Primary();
    }
    Expr.Expr Primary() {
        if(Match(new TokenType[] { TokenType.False })) {
            return new Expr.Literal(false);
        }
        if(Match(new TokenType[] { TokenType.True })) {
            return new Expr.Literal(true);
        }
        if(Match(new TokenType[] { TokenType.Nil })) {
            return new Expr.Literal(null);
        }
        if(Match(new TokenType[] { TokenType.Number, TokenType.String })) {
            return new Expr.Literal(Previous.literal);
        }
        if(Match(new TokenType[] { TokenType.LeftParen })) {
            Expr.Expr expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        throw Error(Peek, "Expect expression.");
    }
    bool Match(TokenType[] tokenTypes) {
        foreach(TokenType type in tokenTypes) {
            if(Check(type)) {
                Advance();
                return true;
            }
        }
        return false;
    }
    bool Check(TokenType type) {
        if(IsAtEnd) {
            return false;
        }
        return Peek.type == type;
    }
    Token Advance() {
        if(!IsAtEnd) {
            current++;
        }
        return Previous;
    }
    Token Consume(TokenType type, string message) {
        if(Check(type)) {
            return Advance();
        }
        throw Error(Peek, message); 
    }
    ParseError Error(Token token, string message) {
        errorReporter.ReportError(token, message);
        return new ParseError();
    }
    void Syncronize() {
        Advance();
        while(!IsAtEnd) {
            if(Previous.type == TokenType.Semicolon) {
                return;
            }
            switch(Peek.type) {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }
            Advance();
        }
    }
}