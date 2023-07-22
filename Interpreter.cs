namespace cslox;

public class Interpreter: Expr.IVisitor<object?> {
    readonly IErrorReporter errorReporter;
    public Interpreter(IErrorReporter errorReporter) {
        this.errorReporter = errorReporter;
    }
    public void Interpret(Expr.Expr expr) {
        try {
            object? value = Evaluate(expr);
            Console.WriteLine(Stringify(value));
        }
        catch(RuntimeError error) {
            errorReporter.RuntimeError(error);
        }
    }
    public object? VisitLiteralExpr(Expr.Literal expr) {
        return expr.value;
    }
    public object? VisitGroupingExpr(Expr.Grouping expr) {
        return Evaluate(expr.expression);
    }
    public object? VisitUnaryExpr(Expr.Unary expr) {
        object? right = Evaluate(expr.right);
        switch(expr.opr.type) {
            case TokenType.Minus:
                double doubleOperand = CheckNumberOperand(expr.opr, right);
                return -doubleOperand;
            case TokenType.Bang:
                return !IsTruthy(right);
        }
        return null;
    }
    public object? VisitBinaryExpr(Expr.Binary expr) {
        object? left = Evaluate(expr.left);
        object? right = Evaluate(expr.right);
        switch(expr.opr.type) {
            case TokenType.Minus: {
                    (double leftDouble, double rightDouble) = CheckNumberOperands(expr.opr, left, right);
                    return leftDouble - rightDouble;
                }
            case TokenType.Plus:
                if(left is double doubleLeft && right is double doubleRight) {
                    return doubleLeft + doubleRight;
                }
                if(left is string stringLeft && right is string stringRight) {
                    return stringLeft + stringRight;
                }
                throw new RuntimeError(expr.opr, "Operands must be two number or two string.");
            case TokenType.Slash: {
                    (double leftDouble, double rightDouble) = CheckNumberOperands(expr.opr, left, right);
                    if(rightDouble == 0) {
                        throw new RuntimeError(expr.opr, "Dividing by zero.");
                    }
                    return leftDouble / rightDouble;
                }
            case TokenType.Star: {
                    (double leftDouble, double rightDouble) = CheckNumberOperands(expr.opr, left, right);
                    return leftDouble * rightDouble;
                }
            case TokenType.Greater: {
                    (double leftDouble, double rightDouble) = CheckNumberOperands(expr.opr, left, right);
                    return leftDouble > rightDouble;
                }
            case TokenType.GreaterEqual: {
                    (double leftDouble, double rightDouble) = CheckNumberOperands(expr.opr, left, right);
                    return leftDouble >= rightDouble;
                }
            case TokenType.Less: {
                    (double leftDouble, double rightDouble) = CheckNumberOperands(expr.opr, left, right);
                    return leftDouble < rightDouble;
                }
            case TokenType.LessEqual: {
                    (double leftDouble, double rightDouble) = CheckNumberOperands(expr.opr, left, right);
                    return leftDouble <= rightDouble;
                }
            case TokenType.EqualEqual:
                return IsEqual(left, right);
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.Comma:
                return right;
        }
        return null;
    }
    public object? VisitTernaryExpr(Expr.Ternary expr) {
        object? first = Evaluate(expr.first);
        if(expr.opr.type == TokenType.Question) {
            if(IsTruthy(first)) {
                return Evaluate(expr.second);
            } else {
                return Evaluate(expr.third);
            }
        }
        return null;
    }
    object? Evaluate(Expr.Expr expr) {
        return expr.Accept(this);
    }
    bool IsTruthy(object? obj) {
        if(obj == null) {
            return false;
        }
        if(obj is bool boolObj) {
            return boolObj;
        }
        return true;
    }
    bool IsEqual(object? a, object? b) {
        if(a is null && b is null) {
            return true;
        }
        if(a is null) {
            return false;
        }
        return a.Equals(b);
    }
    double CheckNumberOperand(Token opr, object? operand) {
        if(operand is double operandDouble) {
            return operandDouble;
        }
        throw new RuntimeError(opr, "Operand must be a number.");
    }
    (double, double) CheckNumberOperands(Token opr, object? left, object? right) {
        if(left is double leftDouble && right is double rightDouble) {
            return (leftDouble, rightDouble);
        }
        throw new RuntimeError(opr, "Operands must be numbers.");
    }
    string Stringify(object? obj) {
        if(obj is null) {
            return "nil";
        }
        if(obj is double objDouble) {
            string text = objDouble.ToString();
            if(text.EndsWith(".0")) {
                text = text.Substring(0, text.Length - 2);
            }
            return text;
        }
        return obj.ToString() ?? string.Empty;
    }
}