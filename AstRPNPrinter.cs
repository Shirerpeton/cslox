using System.Text;

namespace cslox;

public class AstRPNPrinter: Expr.IVisitor<String> {
    public string Print(Expr.Expr expr) {
        return expr.Accept(this);
    }
    string RPNPrint(string name, Expr.Expr[] exprs) {
        var builder = new StringBuilder();
        foreach(Expr.Expr expr in exprs) {
            builder.Append(expr.Accept(this));
            builder.Append(" ");
        }
        builder.Append(name);
        return builder.ToString();
    }

    public string VisitBinaryExpr(Expr.Binary expr) {
        return RPNPrint(expr.opr.lexeme, new Expr.Expr[] { expr.left, expr.right });
    } 
    public string VisitGroupingExpr(Expr.Grouping expr) {
        return  expr.expression.Accept(this);
    } 
    public string VisitLiteralExpr(Expr.Literal expr) {
        if(expr.value == null) {
            return "nil";
        }
        return expr.value.ToString() ?? string.Empty;
    } 
    public string VisitUnaryExpr(Expr.Unary expr) {
        return RPNPrint(expr.opr.lexeme, new Expr.Expr[] { expr.right });
    } 
}