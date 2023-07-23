namespace cslox;

public class Parser {
    readonly List<Token> tokens;
    readonly IErrorReporter errorReporter;
    readonly bool isRepl = false;
    int current = 0;
    bool IsAtEnd => Peek.type == TokenType.EOF;
    Token Peek => tokens.ElementAt(current);
    Token Previous => tokens.ElementAt(current - 1);

    public Parser(IErrorReporter errorReporter, List<Token> tokens, bool isRepl = false) {
        this.errorReporter = errorReporter;
        this.tokens = tokens;
        this.isRepl = isRepl;
    }
    public List<Stmt.Stmt> Parse() {
        var statements = new List<Stmt.Stmt>();
        while(!IsAtEnd) {
            Stmt.Stmt? declaration = Declaration();
            if(declaration != null) {
                statements.Add(declaration);
            }
        }
        return statements;
    }
    Stmt.Stmt? Declaration(bool inALoop = false) {
        try {
            if(Match(new TokenType[] { TokenType.Var })) {
                return VarDeclaration();
            }
            return Statement(inALoop);
        } catch(ParseError) {
            Syncronize();
            return null;
        }
    }
    Stmt.Stmt VarDeclaration() {
        Token name = Consume(TokenType.Identifier, "Expect variable name.");
        Expr.Expr? initializer = null;
        if(Match(new TokenType[] { TokenType.Equal })) {
            initializer = Expression();
        }
        Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }
    Stmt.Stmt Statement(bool inALoop = false) {
        if(Match(new TokenType[] { TokenType.For })) {
            return ForStatement();
        }
        if(Match(new TokenType[] { TokenType.If })) {
            return IfStatement(inALoop);
        }
        if(Match(new TokenType[] { TokenType.Print })) {
            return PrintStatement();
        }
        if(Match(new TokenType[] { TokenType.While })) {
            return WhileStatement();
        }
        if(Match(new TokenType[] { TokenType.LeftBrace })) {
            return new Stmt.Block(Block(inALoop));
        }
        if(Match(new TokenType[] { TokenType.Break })) {
            if(inALoop) {
                return BreakStatement();
            } else {
                throw Error(Previous, "Break statement outside of a loop.");
            }
        }
        if(Match(new TokenType[] { TokenType.Continue })) {
            if(inALoop) {
                return ContinueStatement();
            } else {
                throw Error(Previous, "Continue statement outside of a loop.");
            }
        }
        return ExpressionStatement();
    }
    Stmt.Stmt BreakStatement() {
        Consume(TokenType.Semicolon, "Expect ';' after 'break'.");
        return new Stmt.Break();
    }
    Stmt.Stmt ContinueStatement() {
        Consume(TokenType.Semicolon, "Expect ';' after 'continue'.");
        return new Stmt.Continue();
    }
    Stmt.Stmt ForStatement() {
        Consume(TokenType.LeftParen, "Expect '(' after 'for'.");
        Stmt.Stmt? initializer;
        if(Match(new TokenType[] { TokenType.Semicolon })) {
            initializer = null;
        } else if(Match(new TokenType[] { TokenType.Var })) {
            initializer = VarDeclaration();
        } else {
            initializer = ExpressionStatement();
        }

        Expr.Expr? condition = null;
        if(!Check(TokenType.Semicolon)) {
            condition = Expression();
        }
        Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

        Expr.Expr? increment = null;
        if(!Check(TokenType.RightParen)) {
            increment = Expression();
        }
        Consume(TokenType.RightParen, "Expect ')' after for clauses.");
        Stmt.Stmt body = Statement(inALoop: true);
        return new Stmt.For(initializer, condition, increment, body);
    }
    Stmt.Stmt WhileStatement() {
        Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
        Expr.Expr condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after while condition.");
        Stmt.Stmt body = Statement(inALoop: true);
        return new Stmt.While(condition, body);
    }
    Stmt.Stmt IfStatement(bool inALoop = false) {
        Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        Expr.Expr condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after if condition.");
        Stmt.Stmt thenBranch = Statement(inALoop);
        Stmt.Stmt? elseBranch = null;
        if(Match(new TokenType[] { TokenType.Else })) {
            elseBranch = Statement(inALoop);
        }
        return new Stmt.If(condition, thenBranch, elseBranch);
    }
    Stmt.Stmt PrintStatement() {
        Expr.Expr value = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        return new Stmt.Print(value);
    }
    List<Stmt.Stmt> Block(bool inALoop = false) {
        var statements = new List<Stmt.Stmt>();
        while(!Check(TokenType.RightBrace) && !IsAtEnd) {
            Stmt.Stmt? declaration = Declaration(inALoop);
            if(declaration != null) {
                statements.Add(declaration);
            }
        }
        Consume(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }
    Stmt.Stmt ExpressionStatement() {
        Expr.Expr expr = Expression();
        if(isRepl && !Check(TokenType.Semicolon)) {
            return new Stmt.Print(expr);
        } else {
            Consume(TokenType.Semicolon, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }
    }
    Expr.Expr Expression() {
        return Assignment();
    }
    Expr.Expr Assignment() {
        Expr.Expr expr = CompositeExpr();
        if(Match(new TokenType[] { TokenType.Equal })) {
            Token equals = Previous;
            Expr.Expr value = Assignment();
            if(expr is Expr.Variable exprVariable) {
                Token name = exprVariable.name;
                return new Expr.Assign(name, value);
            }
            Error(equals, "Invalid assignment target.");
        }
        return expr;
    }
    Expr.Expr CompositeExpr() {
        return LABinaryProduction(Conditional,
            new TokenType[] { TokenType.Comma });
    }
    Expr.Expr Conditional() {
        Expr.Expr expr = Or();
        if(Match(new TokenType[] { TokenType.Question })) {
            Token opr = Previous;
            Expr.Expr second = Conditional();
            if(Match(new TokenType[] { TokenType.Colon })) {
                Expr.Expr third = Conditional();
                return new Expr.Ternary(opr, expr, second, third);
            }
            throw Error(Peek, "Unclosed conditional.");
        }
        return expr;
    }
    Expr.Expr Or() {
        Expr.Expr expr = And();
        while(Match(new TokenType[] { TokenType.Or })) {
            Token opr = Previous;
            Expr.Expr right = And();
            expr = new Expr.Logical(expr, opr, right);
        }
        return expr;
    }
    Expr.Expr And() {
        Expr.Expr expr = Equality();
        while(Match(new TokenType[] { TokenType.And })) {
            Token opr = Previous;
            Expr.Expr right = Equality();
            expr = new Expr.Logical(expr, opr, right);
        }
        return expr;
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
    Expr.Expr LABinaryProduction(Func<Expr.Expr> nextProduction, TokenType[] operatorTypes) {
        Expr.Expr expr;
        if(operatorTypes.Contains(Peek.type)) {
            try {
                return Unary();
            }
            catch(ParseError) {
                ParseError error = Error(Peek, "Binary operator without left-hand side.");
                Advance();
                nextProduction();
                throw error;
            }
        } else {
            expr = nextProduction();
            while(Match(operatorTypes)) {
                Token opr = Previous;
                Expr.Expr right = nextProduction();
                expr = new Expr.Binary(expr, opr, right);
            }
            return expr;
        }
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
        if(Match(new TokenType[] { TokenType.Identifier })) {
            return new Expr.Variable(Previous);
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