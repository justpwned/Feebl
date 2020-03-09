using System.Collections.Generic;

namespace Feebl
{
	// 10 concrete
	abstract class Stmt
	{
		public interface Visitor
		{
			void VisitBlockStmt(Block stmt);
			void VisitExpressionStmt(Expression stmt);
			void VisitVarStmt(Var stmt);
			void VisitIfStmt(If stmt);
			void VisitWhileStmt(While stmt);
			void VisitForStmt(For stmt);
			void VisitBreakStmt(Break stmt);
			void VisitContinueStmt(Continue stmt);
			void VisitFunctionStmt(Function stmt);
			void VisitReturnStmt(Return stmt);
		}

		public abstract void Accept(Visitor visitor);
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

		public override void Accept(Visitor visitor) => visitor.VisitReturnStmt(this);
	}

	class Function : Stmt
	{
		public Token Name { get; }
		public List<Token> Params { get; }
		public Stmt Body { get; }

		public Function(Token name, List<Token> parameters, Stmt body)
		{
			Name = name;
			Params = parameters;
			Body = body;
		}

		public override void Accept(Visitor visitor) => visitor.VisitFunctionStmt(this);
	}

	class Break : Stmt
	{
		public override void Accept(Visitor visitor) => visitor.VisitBreakStmt(this);
	}

	class Continue : Stmt
	{
		public override void Accept(Visitor visitor) => visitor.VisitContinueStmt(this);
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

		public override void Accept(Visitor visitor) => visitor.VisitForStmt(this);
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

		public override void Accept(Visitor visitor) => visitor.VisitWhileStmt(this);
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

		public override void Accept(Visitor visitor) => visitor.VisitIfStmt(this);
	}

	class Block : Stmt
	{
		public List<Stmt> Statements { get; }

		public Block(List<Stmt> statements)
		{
			Statements = statements;
		}

		public override void Accept(Visitor visitor) => visitor.VisitBlockStmt(this);
	}

	class Expression : Stmt
	{
		public Expr Expr { get; }

		public Expression(Expr expression)
		{
			Expr = expression;
		}

		public override void Accept(Visitor visitor) => visitor.VisitExpressionStmt(this);
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

		public override void Accept(Visitor visitor) => visitor.VisitVarStmt(this);
	}
}
