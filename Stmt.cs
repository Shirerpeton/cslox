namespace cslox.Stmt;

public abstract class Stmt {
    public abstract void Accept(IVisitor visitor);
}

public interface IVisitor {
    void VisitExpressionStmt(Expression stmt);
    void VisitPrintStmt(Print stmt);
    void VisitVarStmt(Var stmt);
}

public class Expression: Stmt {
    public Expr.Expr expression;
    public Expression(Expr.Expr expression) {
        this.expression = expression;
    }
    public override void Accept(IVisitor visitor) {
        visitor.VisitExpressionStmt(this);
    }
}

public class Print: Stmt {
    public Expr.Expr expression;
    public Print(Expr.Expr expression) {
        this.expression = expression;
    }
    public override void Accept(IVisitor visitor) {
        visitor.VisitPrintStmt(this);
    }
}

public class Var: Stmt {
    public Token name;
    public Expr.Expr? initializer;
    public Var(Token name, Expr.Expr? initializer) {
        this.name = name;
        this.initializer = initializer;
    }
    public override void Accept(IVisitor visitor) {
        visitor.VisitVarStmt(this);
    }
}

