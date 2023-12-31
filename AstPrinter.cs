using System.Text;
namespace cslox;

public class AstPrinter: Expr.IVisitor<String>, Stmt.IVisitor {
    public void Print(List<Stmt.Stmt> statements) {
        Console.WriteLine('(');
        foreach(var statement in statements) {
            statement.Accept(this);
        }
        Console.WriteLine(')');
    }
    public void VisitBlockStmt(Stmt.Block stmt) {
        Console.WriteLine("(");
        foreach(Stmt.Stmt statement in stmt.statements) {
            statement.Accept(this);
        }
        Console.WriteLine(")");
    }
    public void VisitExpressionStmt(Stmt.Expression stmt) {
        Console.WriteLine($"{stmt.expression.Accept(this)}");
    }
    public void VisitVarStmt(Stmt.Var stmt) {
        if(stmt.initializer != null) {
            Console.WriteLine($"(var {stmt.name.lexeme} {stmt.initializer.Accept(this)})");
        } else {
            Console.WriteLine($"(var {stmt.name.lexeme})");
        }
    }
    public void VisitIfStmt(Stmt.If stmt) {
        Console.WriteLine($"(if {stmt.condition.Accept(this)}");
        stmt.thenBranch.Accept(this);
        if(stmt.elseBranch != null) {
            stmt.elseBranch.Accept(this);
        }
        Console.WriteLine(")");
    }
    public void VisitWhileStmt(Stmt.While stmt) {
        Console.WriteLine($"(while {stmt.condition.Accept(this)}");
        stmt.body.Accept(this);
        Console.WriteLine(")");
    }
    public void VisitForStmt(Stmt.For stmt) {
        Console.Write($"(for ");
        if(stmt.initializer != null) {
            stmt.initializer.Accept(this);
        }
        if(stmt.condition != null) {
            Console.WriteLine(stmt.condition.Accept(this));
        }
        if(stmt.increment != null) {
            Console.WriteLine(stmt.increment.Accept(this));
        }
        stmt.body.Accept(this);
        Console.WriteLine(")");
    }
    public void VisitBreakStmt(Stmt.Break stmt) {
        Console.WriteLine("(break)");
    }
    public void VisitContinueStmt(Stmt.Continue stmt) {
        Console.WriteLine("(continue)");
    }
    public void VisitFunctionStmt(Stmt.Function stmt) {
        string parameters = string.Join(' ', stmt.parameters.Select(p => p.lexeme));
        Console.WriteLine($"(define {stmt.name.lexeme} ({parameters})");
        foreach(Stmt.Stmt statement in stmt.body) {
            statement.Accept(this);
        }
        Console.WriteLine(")");
    }
    public void VisitReturnStmt(Stmt.Return stmt) {
        Console.WriteLine("(return)");
    }
    public string Print(Expr.Expr expr) {
        return expr.Accept(this);
    }
    string Parenthesize(string name, Expr.Expr[] exprs) {
        var builder = new StringBuilder();
        builder.Append("(").Append(name);
        foreach(Expr.Expr expr in exprs) {
            builder.Append(" ");
            builder.Append(expr.Accept(this));
        }
        builder.Append(")");
        return builder.ToString();
    }
    public string VisitLambdaExpr(Expr.Lambda expr) {
        string parameters = string.Join(' ', expr.parameters.Select(p => p.lexeme));
        Console.WriteLine($"(lambda {parameters} (");
        foreach(var stmt in expr.body) {
            stmt.Accept(this);
        }
        return $")";
    }
    public string VisitTernaryExpr(Expr.Ternary expr) {
        return Parenthesize(expr.opr.lexeme, new Expr.Expr[] { expr.first, expr.second, expr.third });
    }
    public string VisitBinaryExpr(Expr.Binary expr) {
        return Parenthesize(expr.opr.lexeme, new Expr.Expr[] { expr.left, expr.right });
    }
    public string VisitGroupingExpr(Expr.Grouping expr) {
        return Parenthesize("group", new Expr.Expr[] { expr.expression });
    }
    public string VisitLiteralExpr(Expr.Literal expr) {
        if(expr.value == null) {
            return "nil";
        }
        return expr.value.ToString() ?? string.Empty;
    }
    public string VisitUnaryExpr(Expr.Unary expr) {
        return Parenthesize(expr.opr.lexeme, new Expr.Expr[] { expr.right });
    }
    public string VisitCallExpr(Expr.Call expr) {
        return Parenthesize(expr.callee.Accept(this), expr.arguments.ToArray());
    }
    public string VisitVariableExpr(Expr.Variable expr) {
        return expr.name.lexeme;
    }
    public string VisitAssignExpr(Expr.Assign expr) {
        return Parenthesize($"assign {expr.name.lexeme}", new Expr.Expr[] { expr.value });
    }
    public string VisitLogicalExpr(Expr.Logical expr) {
        return Parenthesize(expr.opr.lexeme, new Expr.Expr[] { expr.left, expr.right });
    }
}
