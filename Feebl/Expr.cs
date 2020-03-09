using System;
using System.Collections.Generic;

namespace Feebl
{
	// 8 concrete
	abstract class Expr
	{
		public interface Visitor<T>
		{
			T VisitAssignExpr(Assign expr);
			T VisitBinaryExpr(Binary expr);
			T VisitLiteralExpr(Literal expr);
			T VisitUnaryExpr(Unary expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitVariableExpr(Variable expr);
			T VisitLogicalExpr(Logical expr);
			T VisitCallExpr(Call expr);
		}

		public abstract T Accept<T>(Visitor<T> visitor);
	}

	class Call : Expr
	{
		public Expr Callee { get; }
		public Token Paren { get; }
		public List<Expr> Arguments { get; }

		public Call(Expr callee, Token paren, List<Expr> args)
		{
			Callee = callee;
			Paren = paren;
			Arguments = args;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitCallExpr(this);
	}

	class Logical : Expr
	{
		public Expr Left { get; }
		public Token Op { get; }
		public Expr Right { get; }

		public Logical(Expr left, Token op, Expr right)
		{
			Left = left;
			Op = op;
			Right = right;
		}

		public override T Accept<T>(Visitor<T> visitor)
		{
			return visitor.VisitLogicalExpr(this);
		}
	}

	class Assign : Expr
	{
		public Token Name { get; }
		public Expr Value { get; }

		public Assign(Token name, Expr value)
		{
			Name = name;
			Value = value;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitAssignExpr(this);
	}

	class Binary : Expr
	{
		public Expr Left { get; }
		public Token Op { get; }
		public Expr Right { get; }

		public Binary(Expr left, Token op, Expr right)
		{
			Left = left;
			Op = op;
			Right = right;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitBinaryExpr(this);
	}

	class Literal : Expr
	{
		public object Value { get; }

		public Literal(Object value)
		{
			Value = value;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitLiteralExpr(this);
	}

	class Unary : Expr
	{
		public Token Op { get; }
		public Expr Right { get; }

		public Unary(Token op, Expr right)
		{
			Op = op;
			Right = right;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitUnaryExpr(this);
	}

	class Grouping : Expr
	{
		public Expr Expression { get; }

		public Grouping(Expr expression)
		{
			Expression = expression;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitGroupingExpr(this);
	}

	class Variable : Expr
	{
		public Token Name { get; }

		public Variable(Token name)
		{
			Name = name;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitVariableExpr(this);
	}
}
