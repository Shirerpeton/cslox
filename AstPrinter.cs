using System.Text;

namespace cslox;

public class AstPrinter: Expr.IVisitor<String> {
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
}