using System.Text;

namespace cslox;

public class AstRPNPrinter: IVisitor<String> {
    public string Print(Expr expr) {
        return expr.Accept(this);
    }
    string RPNPrint(string name, Expr[] exprs) {
        var builder = new StringBuilder();
        foreach(Expr expr in exprs) {
            builder.Append(expr.Accept(this));
            builder.Append(" ");
        }
        builder.Append(name);
        return builder.ToString();
    }

    public string VisitBinaryExpr(Binary expr) {
        return RPNPrint(expr.opr.lexeme, new Expr[] { expr.left, expr.right });
    } 
    public string VisitGroupingExpr(Grouping expr) {
        return RPNPrint("group", new Expr[] { expr.expression });
    } 
    public string VisitLiteralExpr(Literal expr) {
        if(expr.value == null) {
            return "nil";
        }
        return expr.value.ToString() ?? string.Empty;
    } 
    public string VisitUnaryExpr(Unary expr) {
        return RPNPrint(expr.opr.lexeme, new Expr[] { expr.right });
    } 
}