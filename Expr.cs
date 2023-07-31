namespace cslox.Expr;

    public abstract class Expr {
    public abstract R Accept<R>(IVisitor<R> visitor);
}

public interface IVisitor<R> {
    R VisitTernaryExpr(Ternary expr);
    R VisitAssignExpr(Assign expr);
    R VisitBinaryExpr(Binary expr);
    R VisitCallExpr(Call expr);
    R VisitLambdaExpr(Lambda expr);
    R VisitGroupingExpr(Grouping expr);
    R VisitLiteralExpr(Literal expr);
    R VisitLogicalExpr(Logical expr);
    R VisitUnaryExpr(Unary expr);
    R VisitVariableExpr(Variable expr);
}

public class Ternary: Expr {
    public Token opr;
    public Expr first;
    public Expr second;
    public Expr third;
    public Ternary(Token opr, Expr first, Expr second, Expr third) {
        this.opr = opr;
        this.first = first;
        this.second = second;
        this.third = third;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitTernaryExpr(this);
    }
}

public class Assign: Expr {
    public Token name;
    public Expr value;
    public Assign(Token name, Expr value) {
        this.name = name;
        this.value = value;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitAssignExpr(this);
    }
}

public class Binary: Expr {
    public Expr left;
    public Token opr;
    public Expr right;
    public Binary(Expr left, Token opr, Expr right) {
        this.left = left;
        this.opr = opr;
        this.right = right;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitBinaryExpr(this);
    }
}

public class Call: Expr {
    public Expr callee;
    public Token paren;
    public List<Expr> arguments;
    public Call(Expr callee, Token paren, List<Expr> arguments) {
        this.callee = callee;
        this.paren = paren;
        this.arguments = arguments;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitCallExpr(this);
    }
}

public class Lambda: Expr {
    public List<Token> parameters;
    public List<Stmt.Stmt> body;
    public Lambda(List<Token> parameters, List<Stmt.Stmt> body) {
        this.parameters = parameters;
        this.body = body;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitLambdaExpr(this);
    }
}

public class Grouping: Expr {
    public Expr expression;
    public Grouping(Expr expression) {
        this.expression = expression;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitGroupingExpr(this);
    }
}

public class Literal: Expr {
    public object? value;
    public Literal(object? value) {
        this.value = value;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitLiteralExpr(this);
    }
}

public class Logical: Expr {
    public Expr left;
    public Token opr;
    public Expr right;
    public Logical(Expr left, Token opr, Expr right) {
        this.left = left;
        this.opr = opr;
        this.right = right;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitLogicalExpr(this);
    }
}

public class Unary: Expr {
    public Token opr;
    public Expr right;
    public Unary(Token opr, Expr right) {
        this.opr = opr;
        this.right = right;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitUnaryExpr(this);
    }
}

public class Variable: Expr {
    public Token name;
    public Variable(Token name) {
        this.name = name;
    }
    public override R Accept<R>(IVisitor<R> visitor) {
        return visitor.VisitVariableExpr(this);
    }
}



