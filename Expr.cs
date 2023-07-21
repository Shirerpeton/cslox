namespace cslox;

public abstract class Expr {
}

public class Binary: Expr {
    Expr left;
    Token opr;
    Expr right;

    public Binary(Expr left, Token opr, Expr right) {
        this.left = left;
        this.opr = opr;
        this.right = right;
    }
}

public class Grouping: Expr {
    Expr expression;

    public Grouping(Expr expression) {
        this.expression = expression;
    }
}

public class Literal: Expr {
    object value;

    public Literal(object value) {
        this.value = value;
    }
}

public class Unary: Expr {
    Token opr;
    Expr right;

    public Unary(Token opr, Expr right) {
        this.opr = opr;
        this.right = right;
    }
}

