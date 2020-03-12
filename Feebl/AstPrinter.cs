using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feebl
{
	class AstPrinter : Expr.Visitor<string>, Stmt.Visitor<string>
	{
		private int currentOffset = 0;

		public string Print(Expr expr)
		{
			return expr.Accept(this);
		}

		public string Print(Stmt stmt)
		{
			return stmt.Accept(this);
		}

		private string Parenthesize(string name, params Expr[] exprs)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("(").Append(name);

			for (int i = 0; i < exprs.Length; ++i)
			{
				sb.Append(" ");
				sb.Append(exprs[i].Accept(this));
			}

			sb.Append(")");

			return sb.ToString();
		}

		private string Parenthesize2(string name, params object[] parts)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("(").Append(name);

			int prevOffset = currentOffset;
			currentOffset += name.Length + 2;

			for (int i = 0; i < parts.Length; ++i)
			{
				object part = parts[i];
				if (part is Expr)
				{
					Expr expr = part as Expr;
					sb.Append(" ");
					sb.Append(expr.Accept(this));
					sb.Append(" ");
				}
				else if (part is Stmt)
				{
					//sb.Append(" ");
					//currentOffset++;
					if (i != parts.Length)
					{
						sb.AppendLine();
						sb.Append(new string(' ', currentOffset));
					}
					sb.Append((part as Stmt).Accept(this));
				}
				else if (part is List<Expr>)
				{
					foreach (var expr in (part as List<Expr>))
					{
						sb.Append(" ");
						sb.Append(Parenthesize(expr.Accept(this)));
					}
				}
				else
				{
					sb.Append(" ");
					sb.Append(part);
				}
			}

			currentOffset = prevOffset;

			sb.Append(")");

			return sb.ToString();
		}

		public string VisitAssignExpr(Assign expr)
		{
			return Parenthesize2("=", expr.Name.Lexeme, expr.Value);
		}

		public string VisitBinaryExpr(Binary expr)
		{
			return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
		}

		public string VisitBlockStmt(Block stmt)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("(block ");

			int prevOffset = currentOffset;
			currentOffset += 7;
			for (int i = 0; i < stmt.Statements.Count; ++i)
			{
				sb.Append(stmt.Statements[i].Accept(this));
				if (i != stmt.Statements.Count - 1)
				{
					sb.AppendLine();
					sb.Append(new string(' ', currentOffset));
				}
			}
			currentOffset = prevOffset;

			sb.Append(")");
			return sb.ToString();
		}

		public string VisitBreakStmt(Break stmt)
		{
			return "(break)";
		}

		public string VisitCallExpr(Call expr)
		{
			return Parenthesize2("call", expr.Callee, expr.Arguments);
		}

		public string VisitContinueStmt(Continue stmt)
		{
			return "(continue)";
		}

		public string VisitExpressionStmt(Expression stmt)
		{
			return Parenthesize(";", stmt.Expr);
		}

		public string VisitForStmt(For stmt)
		{
			return Parenthesize2("for", stmt.Condition, stmt.Inc, stmt.Body);
		}

		public string VisitFunctionStmt(Function stmt)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("(func ").Append(stmt.Name.Lexeme).Append("(");

			foreach (var param in stmt.Params)
			{
				if (param != stmt.Params[0]) sb.Append(" ");
				sb.Append(param.Lexeme);
			}

			sb.Append(") ");

			int prevOffset = currentOffset;
			currentOffset += 6;
			for (int i = 0; i < stmt.Body.Statements.Count; ++i)
			{
				if (i != stmt.Body.Statements.Count - 1)
				{
					sb.AppendLine();
					sb.Append(new string(' ', currentOffset));
				}
				sb.Append(stmt.Body.Statements[i].Accept(this));
			}
			currentOffset = prevOffset;

			sb.Append(")");
			
			return sb.ToString();
		}

		public string VisitGroupingExpr(Grouping expr)
		{
			return Parenthesize("group", expr.Expression);
		}

		public string VisitIfStmt(If stmt)
		{
			if (stmt.ElseBranch == null)
			{
				return Parenthesize2("if", stmt.Condition, stmt.ThenBranch);
			}

			return Parenthesize2("if-else", stmt.Condition, stmt.ThenBranch, stmt.ElseBranch);
		}

		public string VisitLiteralExpr(Literal expr)
		{
			string literal = "nil";
			if (expr.Value != null)
			{
				literal = expr.Value.ToString();
			}
			//currentOffset += literal.Length + 1;
			return literal;
		}

		public string VisitLogicalExpr(Logical expr)
		{
			return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
		}

		public string VisitReturnStmt(Return stmt)
		{
			if (stmt.Value == null) return "(return)";
			return Parenthesize("return", stmt.Value);
		}

		public string VisitUnaryExpr(Unary expr)
		{
			return Parenthesize(expr.Op.Lexeme, expr.Right);
		}

		public string VisitVariableExpr(Variable expr)
		{
			return expr.Name.Lexeme;
		}

		public string VisitVarStmt(Var stmt)
		{
			if (stmt.Initializer == null)
			{
				return Parenthesize2("var", stmt.Name);
			}

			return Parenthesize2("var", stmt.Name, "=", stmt.Initializer);
		}

		public string VisitWhileStmt(While stmt)
		{
			return Parenthesize2("while", stmt.Condition, stmt.Body);
		}
	}
}
