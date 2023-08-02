namespace cslox;

public struct None {}

public class Resolver: Expr.IVisitor<None>, Stmt.IVisitor {
    readonly Interpreter interpreter;
    readonly IErrorReporter errorReporter;
    Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
    public Resolver(IErrorReporter errorReporter, Interpreter interpreter) {
        this.errorReporter = errorReporter;
        this.interpreter = interpreter;
    }
    public void VisitBlockStmt(Stmt.Block stmt) {
        BeginScope();
        Resolve(stmt.statements);
        EndScope();
    }
    public void Resolve(List<Stmt.Stmt> statements) {
        foreach(Stmt.Stmt statement in statements) {
            Resolve(statement);
        }
    }
    void Resolve(Stmt.Stmt stmt) {
        stmt.Accept(this);
    }
    void Resolve(Expr.Expr expr) {
        expr.Accept(this);
    }
    void BeginScope() {
        scopes.Push(new Dictionary<string, bool>());
    }
    void EndScope() {
        scopes.Pop();
    }
    void Declare(Token name) {
        if(scopes.Count == 0) {
            return;
        }
        Dictionary<string, bool> scope = scopes.Peek();
        scope.Add(name.lexeme, false);
    }
    void Define(Token name) {
        if(scopes.Count == 0) {
            return;
        }
        Dictionary<string, bool> scope = scopes.Peek();
        if(!scope.TryGetValue(name.lexeme, out bool defined)) {
            throw new Exception();
        }
        scope[name.lexeme] = true;
    }
    void ResolveLocal(Expr.Expr expr, Token name) {
        for(int i = scopes.Count - 1; i >= 0; i--) {
            if(scopes.ElementAt(i).ContainsKey(name.lexeme)) {
                interpreter.Resolve(expr, scopes.Count - 1 - i);
                return;
            }
        }
    }
    void ResolveFunction(Stmt.Function function) {
        BeginScope();
        foreach(Token param in function.parameters) {
            Declare(param);
            Define(param);
        }
        Resolve(function.body);
        EndScope();
    }
    public void VisitVarStmt(Stmt.Var stmt) {
        Declare(stmt.name);
        if(stmt.initializer != null) {
            Resolve(stmt.initializer);
        }
        Define(stmt.name);
    }
    public None VisitVariableExpr(Expr.Variable expr) {
        if(!(scopes.Count == 0) &&
            scopes.Peek().TryGetValue(expr.name.lexeme, out bool defined) &&
            defined == false) {
            errorReporter.ReportError(expr.name, "Can't read local variable in its own initializer.");
        }
        ResolveLocal(expr, expr.name);
        return new None();
    }
    public None VisitAssignExpr(Expr.Assign expr) {
        Resolve(expr.value);
        ResolveLocal(expr, expr.name);
        return new None();
    }
    public void VisitFunctionStmt(Stmt.Function stmt) {
        Declare(stmt.name);
        Define(stmt.name);
        ResolveFunction(stmt);
    }
    public void VisitExpressionStmt(Stmt.Expression stmt) {
        Resolve(stmt.expression);
    }
    public void VisitIfStmt(Stmt.If stmt) {
        Resolve(stmt.condition);
        Resolve(stmt.thenBranch);
        if(stmt.elseBranch != null) {
            Resolve(stmt.elseBranch);
        }
    }
    public void VisitPrintStmt(Stmt.Print stmt) {
        Resolve(stmt.expression);
    }
    public void VisitReturnStmt(Stmt.Return stmt) {
        if(stmt.value != null) {
            Resolve(stmt.value);
        }
    }
    public void VisitWhileStmt(Stmt.While stmt) {
        Resolve(stmt.condition);
        Resolve(stmt.body);
    }
    public None VisitBinaryExpr(Expr.Binary expr) {
        Resolve(expr.left);
        Resolve(expr.right);
        return new None();
    }
    public None VisitCallExpr(Expr.Call expr) {
        Resolve(expr.callee);
        foreach(Expr.Expr argument in expr.arguments) {
            Resolve(argument);
        }
        return new None();
    }
    public None VisitGroupingExpr(Expr.Grouping expr) {
        Resolve(expr.expression);
        return new None();
    }
    public None VisitLiteralExpr(Expr.Literal expr) {
        return new None();
    }
    public None VisitLogicalExpr(Expr.Logical expr) {
        Resolve(expr.left);
        Resolve(expr.right);
        return new None();
    }
    public None VisitUnaryExpr(Expr.Unary expr) {
        Resolve(expr.right);
        return new None();
    }
}
