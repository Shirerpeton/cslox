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
    void VisitWhileStmt(While stmt);
    void VisitForStmt(For stmt);
    void VisitBreakStmt(Break stmt);
    void VisitContinueStmt(Continue stmt);
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

public class While: Stmt {
    public Expr.Expr condition;
    public Stmt body;
    public While(Expr.Expr condition, Stmt body) {
        this.condition = condition;
        this.body = body;
    }
    public override void Accept(IVisitor visitor) {
        visitor.VisitWhileStmt(this);
    }
}

public class For: Stmt {
    public Stmt? initializer;
    public Expr.Expr? condition;
    public Expr.Expr? increment;
    public Stmt body;
    public For(Stmt? initializer, Expr.Expr? condition, Expr.Expr? increment, Stmt body) {
        this.initializer = initializer;
        this.condition = condition;
        this.increment = increment;
        this.body = body;
    }
    public override void Accept(IVisitor visitor) {
        visitor.VisitForStmt(this);
    }
}

public class Break: Stmt {
    public override void Accept(IVisitor visitor) {
        visitor.VisitBreakStmt(this);
    }
}

public class Continue: Stmt {
    public override void Accept(IVisitor visitor) {
        visitor.VisitContinueStmt(this);
    }
}

