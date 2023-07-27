namespace cslox;

public class Interpreter: Expr.IVisitor<object?>, Stmt.IVisitor {
    bool isBreak;
    bool isContinue;
    readonly IErrorReporter errorReporter;
    public Environment globals = new Environment();
    Environment environment;
    public Interpreter(IErrorReporter errorReporter) {
        this.errorReporter = errorReporter;
        this.globals.Define("clock", new GlobalFunction(arity: 0,
            (Interpreter interpreter, List<object?> arguments) => {
                return (double)(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            }
        ));
        this.globals.Define("print", new GlobalFunction(arity: 1,
            (Interpreter interpreter, List<object?> arguments) => {
                string arg = Stringify(arguments[0]);
                Console.WriteLine(arg);
                return arg;
            }));
        this.environment = globals;
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
    public void ExecuteBlock(List<Stmt.Stmt> statements, Environment environment) {
        Environment previous = this.environment;
        try {
            this.environment = environment;
            foreach(Stmt.Stmt statement in statements) {
                Execute(statement);
                if(isBreak || isContinue) {
                    break;
                }
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
        environment.Define(stmt.name.lexeme, value, initialize: stmt.initializer != null);
    }
    public void VisitWhileStmt(Stmt.While stmt) {
        while(IsTruthy(Evaluate(stmt.condition))) {
            Execute(stmt.body);
            if(isContinue) {
                isContinue = false;
            }
            if(isBreak) {
                isBreak = false;
                break;
            }
        }
    }
    public void VisitForStmt(Stmt.For stmt) {
        Expr.Expr condition = stmt.condition != null ? stmt.condition : new Expr.Literal(true);
        Environment previous = this.environment;
        try {
            this.environment = new Environment(previous);
            if(stmt.initializer != null) {
                Execute(stmt.initializer);
            }
            while(IsTruthy(Evaluate(condition))) {
                Execute(stmt.body);
                if(stmt.increment != null) {
                    Evaluate(stmt.increment);
                }
                if(isContinue) {
                    isContinue = false;
                }
                if(isBreak) {
                    isBreak = false;
                    break;
                }
            }
        }
        finally {
            environment = previous;
        }
    }
    public void VisitBreakStmt(Stmt.Break stmt) {
        isBreak = true;
    }
    public void VisitContinueStmt(Stmt.Continue stmt) {
        isContinue = true;
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
    public void VisitFunctionStmt(Stmt.Function stmt) {
        var function = new Function(stmt, environment);
        environment.Define(stmt.name.lexeme, function);
    }
    public void VisitReturnStmt(Stmt.Return stmt) {
        object? value = null;
        if(stmt.value != null) {
            value = Evaluate(stmt.value);
        }
        throw new Return(value);
    }
    public object? VisitLambdaExpr(Expr.Lambda expr) {
        return new Lambda(expr, environment);
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
    public object? VisitCallExpr(Expr.Call expr) {
        object? callee = Evaluate(expr.callee);
        var arguments = new List<object?>();
        foreach(Expr.Expr argument in expr.arguments) {
            arguments.Add(Evaluate(argument));
        }
        if(callee is ICallable callable) {
            if(arguments.Count != callable.Arity) {
                throw new RuntimeError(expr.paren, $"Expected {callable.Arity} arguments but got {arguments.Count}.");
            }
            return callable.Call(this, arguments);
        }
        throw new RuntimeError(expr.paren, "Can only call functions and classes.");
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
            case TokenType.Plus: {
                    if(left is double leftDouble && right is double rightDouble) {
                        return leftDouble + rightDouble;
                    }
                    if(left is string leftString && right is string rightString) {
                        return leftString + rightString;
                    } /*else if(left is string leftString1) {
                    return leftString1 + Stringify(right);
                } else if(right is string rigthString1) {
                    return Stringify(left) + rigthString1;
                }*/
                    throw new RuntimeError(expr.opr, "Operands must be two number or two string.");
                }
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
