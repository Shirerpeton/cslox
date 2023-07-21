using System.Text;

namespace cslox;

public class AstPrinter: IVisitor<String> {
    string Print(Expr expr) {
        return expr.Accept(this);
    }
    string Parenthesize(string name, Expr[] exprs) {
        var builder = new StringBuilder();
        builder.Append("(").Append(name);
        foreach(Expr expr in exprs) {
            builder.Append(" ");
            builder.Append(expr.Accept(this));
        }
        builder.Append(")");
        return builder.ToString();
    }

    public string VisitBinaryExpr(Binary expr) {
        return Parenthesize(expr.opr.lexeme, new Expr[] { expr.left, expr.right });
    } 
    public string VisitGroupingExpr(Grouping expr) {
        return Parenthesize("group", new Expr[] { expr.expression });
    } 
    public string VisitLiteralExpr(Literal expr) {
        if(expr.value == null) {
            return "nil";
        }
        return expr.value.ToString() ?? string.Empty;
    } 
    public string VisitUnaryExpr(Unary expr) {
        return Parenthesize(expr.opr.lexeme, new Expr[] { expr.right });
    } 
}