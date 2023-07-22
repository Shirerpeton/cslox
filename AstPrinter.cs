using System.Text;

namespace cslox;
public class AstPrinter: Expr.IVisitor<String>, Stmt.IVisitor {
    public void Print(List<Stmt.Stmt> statements) {
        foreach(var statement in statements) {
            statement.Accept(this);
        }
    }
    public void VisitBlockStmt(Stmt.Block stmt) {
        Console.WriteLine("{");
        foreach(Stmt.Stmt statement in stmt.statements) {
            statement.Accept(this);
        }
        Console.WriteLine("}");
    }
    public void VisitPrintStmt(Stmt.Print stmt) {
        Console.WriteLine($"print {stmt.expression.Accept(this)};");
    }
    public void VisitExpressionStmt(Stmt.Expression stmt) {
        Console.WriteLine($"{stmt.expression.Accept(this)};");
    }
    public void VisitVarStmt(Stmt.Var stmt) {
        if(stmt.initializer != null) {
            Console.WriteLine($"var {stmt.name} = {stmt.initializer.Accept(this)};");
        } else {
            Console.WriteLine($"var {stmt.name};");
        }
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
    public string VisitVariableExpr(Expr.Variable expr) {
        return expr.name.lexeme;
    }
    public string VisitAssignExpr(Expr.Assign expr) {
        return Parenthesize($"assign {expr.name} ", new Expr.Expr[] { expr.value });
    }
}