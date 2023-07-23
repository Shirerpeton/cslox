namespace cslox;

public class Interpreter: Expr.IVisitor<object?>, Stmt.IVisitor {
    readonly IErrorReporter errorReporter;
    Environment environment = new Environment();
    public Interpreter(IErrorReporter errorReporter) {
        this.errorReporter = errorReporter;
    }
    public void Interpret(List<Stmt.Stmt> statements) {
        try {
            foreach(var statement in statements) {
                Execute(statement);
            }
        }
        catch(RuntimeError error) {
            errorReporter.RuntimeError(error);
        }
    }
    void Execute(Stmt.Stmt stmt) {
        stmt.Accept(this);
    }
    void ExecuteBlock(List<Stmt.Stmt> statements, Environment environment) {
        Environment previous = this.environment;
        try {
            this.environment = environment;
            foreach(Stmt.Stmt statement in statements) {
                Execute(statement);
            }
        }
        finally {
            this.environment = previous;
        }
    }
    object? Evaluate(Expr.Expr expr) {
        return expr.Accept(this);
    }
    public void VisitBlockStmt(Stmt.Block stmt) {
        ExecuteBlock(stmt.statements, new Environment(environment));
    }
    public void VisitVarStmt(Stmt.Var stmt) {
        object? value = null;
        if(stmt.initializer != null) {
            value = Evaluate(stmt.initializer);
        }
        environment.Define(stmt.name.lexeme, value);
    }
    public void VisitExpressionStmt(Stmt.Expression stmt) {
        Evaluate(stmt.expression);
    }
    public void VisitIfStmt(Stmt.If stmt) {
        if(IsTruthy(Evaluate(stmt.condition))) {
            Execute(stmt.thenBranch);
        } else if(stmt.elseBranch != null) {
            Execute(stmt.elseBranch);
        }
    }
    public void VisitPrintStmt(Stmt.Print stmt) {
        object? value = Evaluate(stmt.expression);
        Console.WriteLine(Stringify(value));
    }
    public object? VisitLogicalExpr(Expr.Logical expr) {
        object? left = Evaluate(expr.left);
        if(expr.opr.type == TokenType.Or) {
            if(IsTruthy(left)) {
                return left;
            }
        } else {
            if(!IsTruthy(left)) {
                return left;
            }
        }
        return Evaluate(expr.right);
    }
    public object? VisitAssignExpr(Expr.Assign expr) {
        object? value = Evaluate(expr.value);
        environment.Assign(expr.name, value);
        return value;
    }
    public object? VisitVariableExpr(Expr.Variable expr) {
        return environment.Get(expr.name);
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
        }
        return null;
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