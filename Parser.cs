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
        return CompositeExpr();
    }
    Expr.Expr CompositeExpr() {
        return LABinaryProduction(Equality,
            new TokenType[] { TokenType.Comma });
    }
    Expr.Expr Equality() {
        return LABinaryProduction(Comparison,
            new TokenType[] { TokenType.BangEqual, TokenType.EqualEqual });
    }
    Expr.Expr Comparison() {
        return LABinaryProduction(Term,
            new TokenType[] { TokenType.Less, TokenType.LessEqual, TokenType.Greater, TokenType.GreaterEqual });
    }
    Expr.Expr Term() {
        return LABinaryProduction(Factor,
            new TokenType[] { TokenType.Minus, TokenType.Plus });
    }
    Expr.Expr Factor() {
        return LABinaryProduction(Unary,
            new TokenType[] { TokenType.Slash, TokenType.Star });
    }
    //Helper for left-associated binary productions
    Expr.Expr LABinaryProduction(Func<Expr.Expr> nextProduction, TokenType[] tokenTypes) {
        Expr.Expr expr = nextProduction();
        while(Match(tokenTypes)) {
            Token opr = Previous;
            Expr.Expr right = nextProduction();
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