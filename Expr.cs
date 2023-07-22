namespace cslox.Expr;

public abstract class Expr {
    public abstract R Accept<R>(IVisitor<R> visitor);
}

public interface IVisitor<R> {
    R VisitTernaryExpr(Ternary expr);
    R VisitBinaryExpr(Binary expr);
    R VisitGroupingExpr(Grouping expr);
    R VisitLiteralExpr(Literal expr);
    R VisitUnaryExpr(Unary expr);
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

