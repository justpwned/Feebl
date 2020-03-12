using System.Collections.Generic;

namespace Feebl
{
	// 10 concrete
	abstract class Stmt
	{
		public interface Visitor<T>
		{
			T VisitBlockStmt(Block stmt);
			T VisitExpressionStmt(Expression stmt);
			T VisitVarStmt(Var stmt);
			T VisitIfStmt(If stmt);
			T VisitWhileStmt(While stmt);
			T VisitForStmt(For stmt);
			T VisitBreakStmt(Break stmt);
			T VisitContinueStmt(Continue stmt);
			T VisitFunctionStmt(Function stmt);
			T VisitReturnStmt(Return stmt);
		}

		public abstract T Accept<T>(Visitor<T> visitor);
	}

	class Return : Stmt
	{
		public Token Keyword { get; }
		public Expr Value { get; }

		public Return(Token keyword, Expr value)
		{
			Keyword = keyword;
			Value = value;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitReturnStmt(this);
	}

	class Function : Stmt
	{
		public Token Name { get; }
		public List<Token> Params { get; }
		public Block Body { get; }

		public Function(Token name, List<Token> parameters, Block body)
		{
			Name = name;
			Params = parameters;
			Body = body;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitFunctionStmt(this);
	}

	class Break : Stmt
	{
		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitBreakStmt(this);
	}

	class Continue : Stmt
	{
		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitContinueStmt(this);
	}

	class For : Stmt
	{
		public Expr Condition { get; }
		public Stmt Inc { get; }
		public Stmt Body { get; }

		public For(Expr cond, Stmt inc, Stmt body)
		{
			Condition = cond;
			Inc = inc;
			Body = body;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitForStmt(this);
	}

	class While : Stmt
	{
		public Expr Condition { get; }
		public Stmt Body { get; }

		public While(Expr cond, Stmt body)
		{
			Condition = cond;
			Body = body;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitWhileStmt(this);
	}

	class If : Stmt
	{
		public Expr Condition { get; }
		public Stmt ThenBranch { get; }
		public Stmt ElseBranch { get; }

		public If(Expr cond, Stmt thenBranch, Stmt elseBranch)
		{
			Condition = cond;
			ThenBranch = thenBranch;
			ElseBranch = elseBranch;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitIfStmt(this);
	}

	class Block : Stmt
	{
		public List<Stmt> Statements { get; }

		public Block(List<Stmt> statements)
		{
			Statements = statements;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitBlockStmt(this);
	}

	class Expression : Stmt
	{
		public Expr Expr { get; }

		public Expression(Expr expression)
		{
			Expr = expression;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitExpressionStmt(this);
	}

	class Var : Stmt
	{
		public Token Name { get; }
		public Expr Initializer { get; }

		public Var(Token name, Expr initializer)
		{
			Name = name;
			Initializer = initializer;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitVarStmt(this);
	}
}
