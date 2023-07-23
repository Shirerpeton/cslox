namespace cslox.Stmt;

public abstract class Stmt {
    public abstract void Accept(IVisitor visitor);
}

public interface IVisitor {
    void VisitBlockStmt(Block stmt);
    void VisitExpressionStmt(Expression stmt);
    void VisitIfStmt(If stmt);
    void VisitPrintStmt(Print stmt);
    void VisitVarStmt(Var stmt);
}

public class Block: Stmt {
    public List<Stmt> statements;
    public Block(List<Stmt> statements) {
        this.statements = statements;
    }
    public override void Accept(IVisitor visitor) {
        visitor.VisitBlockStmt(this);
    }
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

public class If: Stmt {
    public Expr.Expr condition;
    public Stmt thenBranch;
    public Stmt? elseBranch;
    public If(Expr.Expr condition, Stmt thenBranch, Stmt? elseBranch) {
        this.condition = condition;
        this.thenBranch = thenBranch;
        this.elseBranch = elseBranch;
    }
    public override void Accept(IVisitor visitor) {
        visitor.VisitIfStmt(this);
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

