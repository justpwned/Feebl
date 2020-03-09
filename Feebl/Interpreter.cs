using System;
using System.Collections.Generic;

namespace Feebl
{
	class Interpreter : Expr.Visitor<object>, Stmt.Visitor
	{
		public Environment Globals { get; } = new Environment();

		private Environment environment;
		private object uninitialized = new object();
		private Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

		public Interpreter()
		{
			environment = Globals;
			Globals.Define("clock",   new Library.Clock());
			Globals.Define("print",   new Library.IO.Print());
			Globals.Define("println", new Library.IO.Println());
			Globals.Define("read",    new Library.IO.Read());
			Globals.Define("exit",    new Library.Exit());
		}

		public void Interpret(List<Stmt> statements)
		{
			try
			{
				foreach (var stmt in statements)
				{
					Execute(stmt);
				}
			}
			catch (RuntimeException e)
			{
				Feebl.RuntimeError(e);
			}
		}

		public string Interpret(Expr expression)
		{
			try
			{
				object value = Evaluate(expression);
				return Stringify(value);
			}
			catch (RuntimeException e)
			{
				Feebl.RuntimeError(e);
				return null;
			}
		}

		//public void Resolve(Expr expr, int depth)
		//{
		//	locals[expr] = depth;
		//}

		private void Execute(Stmt stmt)
		{
			stmt.Accept(this);
		}

		private string Stringify(object obj)
		{
			if (obj == null)
			{
				return "nil";
			}

			if (obj is double)
			{
				string text = obj.ToString();
				if (text.EndsWith(".0"))
				{
					text = text.Substring(0, text.Length - 2);
				}
				return text;
			}

			return obj.ToString();
		}

		public object VisitAssignExpr(Assign expr)
		{
			object value = Evaluate(expr.Value);
			environment.Assign(expr.Name, value);
			return value;
		}

		public object VisitGroupingExpr(Grouping expr)
		{
			return Evaluate(expr.Expression);
		}

		public object VisitLiteralExpr(Literal expr)
		{
			return expr.Value;
		}

		public object VisitBinaryExpr(Binary expr)
		{
			object left = Evaluate(expr.Left);
			object right = Evaluate(expr.Right);

			switch (expr.Op.Type)
			{
				case TokenType.BIT_AND:
					CheckNumberOperands(expr.Op, left, right, true);
					return Convert.ToInt32(left) & Convert.ToInt32(right);
				case TokenType.BIT_INC_OR:
					CheckNumberOperands(expr.Op, left, right, true);
					return Convert.ToInt32(left) | Convert.ToInt32(right);
				case TokenType.BIT_EX_OR:
					CheckNumberOperands(expr.Op, left, right, true);
					return Convert.ToInt32(left) ^ Convert.ToInt32(right);
				case TokenType.GREATER:
					CheckNumberOperands(expr.Op, left, right);
					return (double)left > (double)right;
				case TokenType.GREATER_EQUAL:
					CheckNumberOperands(expr.Op, left, right);
					return (double)left >= (double)right;
				case TokenType.LESS:
					CheckNumberOperands(expr.Op, left, right);
					return (double)left < (double)right;
				case TokenType.LESS_EQUAL:
					CheckNumberOperands(expr.Op, left, right);
					return (double)left <= (double)right;
				case TokenType.MINUS:
					CheckNumberOperands(expr.Op, left, right);
					return (double)left - (double)right;
				case TokenType.EQUAL_EQUAL:
					return IsEqual(left, right);
				case TokenType.BANG_EQUAL:
					return !IsEqual(left, right);
				case TokenType.PLUS:
					if (left is double && right is double)
						return (double)left + (double)right;
					if (left is string && right is string)
						return (string)left + (string)right;
					throw new RuntimeException(expr.Op, "Operands must be two numbers or two strings.");
				case TokenType.STAR:
					CheckNumberOperands(expr.Op, left, right);
					return (double)left * (double)right;
				case TokenType.SLASH:
					CheckNumberOperands(expr.Op, left, right);
					return (double)left / (double)right;
				case TokenType.PERCENT:
					CheckNumberOperands(expr.Op, left, right);
					return (double)left % (double)right;
			}

			return null;
		}

		public object VisitUnaryExpr(Unary expr)
		{
			object right = Evaluate(expr.Right);

			switch (expr.Op.Type)
			{
				case TokenType.BANG:
					return !IsTruthy(right);
				case TokenType.PLUS:
					CheckNumberOperand(expr.Op, right);
					return (double)right;
				case TokenType.MINUS:
					CheckNumberOperand(expr.Op, right);
					return -(double)right;
				
				default:
					return null;
			}
		}

		private bool IsTruthy(object obj)
		{
			if (obj == null) return false;
			if (obj is bool) return (bool)obj;
			if (obj is double) return (double)obj != 0.0;
			if (obj is string) return ((string)obj).Length != 0;
			return true;
		}

		private bool IsEqual(object a, object b)
		{
			if (a == null && b == null) return true;
			if (a == null || b == null) return false;
			return a.Equals(b);
		}

		private void CheckNumberOperand(Token op, object operand)
		{
			if (!(operand is double))
			{
				throw new RuntimeException(op, "Operand must be a number.");
			}
		}

		private void CheckNumberOperands(Token op, object left, object right, bool checkIfInt=false)
		{
			if (!(left is double && right is double))
			{
				throw new RuntimeException(op, "Operands must be numbers.");
			}

			if (checkIfInt)
			{
				bool isLeftInt = Convert.ToInt32(left) == (double)left;
				bool isRightInt = Convert.ToInt32(right) == (double)right;
				if (!isLeftInt || !isRightInt)
				{
					throw new RuntimeException(op, "Operands must be integer numbers.");
				}
			}
		}

		private object Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}

		public void VisitExpressionStmt(Expression stmt)
		{
			Evaluate(stmt.Expr);
		}

		private object LookUpVariable(Token name, Variable expr)
		{
			return environment.Get(name);
			//if (locals.ContainsKey(expr))
			//{
			//	return environment.GetAt(locals[expr], name.Lexeme);
			//}
			//else
			//{
			//	return Globals.Get(name);
			//}
		}

		public object VisitVariableExpr(Variable expr)
		{
			return LookUpVariable(expr.Name, expr);
			// object value = environment.Get(expr.Name);
			// if (value == uninitialized)
			// {
			// 	throw new RuntimeException(expr.Name, "Variable must be initialized before use.");
			// }
			// return value;
		}

		public void VisitVarStmt(Var stmt)
		{
			object value = uninitialized;
			if (stmt.Initializer != null)
			{
				value = Evaluate(stmt.Initializer);
			}

			environment.Define(stmt.Name.Lexeme, value);
		}

		public void VisitBlockStmt(Block stmt)
		{
			ExecuteBlock(stmt.Statements, new Environment(environment));
		}

		public void ExecuteBlock(List<Stmt> statements, Environment environment)
		{
			Environment current = this.environment;
			try
			{
				this.environment = environment;

				foreach (var stmt in statements)
				{
					Execute(stmt);
				}
			}
			finally
			{
				this.environment = current;
			}
		}

		public void VisitIfStmt(If stmt)
		{
			if (IsTruthy(Evaluate(stmt.Condition)))
			{
				Execute(stmt.ThenBranch);
			}
			else if (stmt.ElseBranch != null)
			{
				Execute(stmt.ElseBranch);
			}
		}

		public object VisitLogicalExpr(Logical expr)
		{
			object left = Evaluate(expr.Left);
			if (expr.Op.Type == TokenType.LOG_OR)
			{
				if (IsTruthy(left)) return left;
			}
			else
			{
				if (!IsTruthy(left)) return left;
			}

			return Evaluate(expr.Right);
		}

		public void VisitWhileStmt(While stmt)
		{
			try
			{
				while (IsTruthy(Evaluate(stmt.Condition)))
				{	
					try
					{
						Execute(stmt.Body);
					}
					catch (ContinueException e) { }
				}
			}
			catch (BreakException e) { }
		}

		public void VisitForStmt(For stmt)
		{
			try
			{
				while (IsTruthy(Evaluate(stmt.Condition)))
				{
					try
					{
						Execute(stmt.Body);
					}
					catch (ContinueException e) { }
					finally
					{
						if (stmt.Inc != null)
						{
							Execute(stmt.Inc);
						}
					}
				}
			}
			catch (BreakException e) { }
		}

		public void VisitBreakStmt(Break stmt)
		{
			throw new BreakException();
		}

		public void VisitContinueStmt(Continue stmt)
		{
			throw new ContinueException();
		}

		public object VisitCallExpr(Call expr)
		{
			object callee = Evaluate(expr.Callee);

			List<object> args = new List<object>();
			foreach (var arg in expr.Arguments)
			{
				args.Add(Evaluate(arg));
			}

			if (!(callee is FeeblCallable))
			{
				throw new RuntimeException(expr.Paren, "Can only call functions and classes.");
			}

			FeeblCallable function = (FeeblCallable)callee;


			if (function is Library.IO.Print)
			{
				((Library.IO.Print)function).Arity = args.Count;
			}
			else if (function is Library.IO.Println)
			{
				((Library.IO.Println)function).Arity = args.Count;
			}

			if (args.Count != function.Arity)
			{
				throw new RuntimeException(expr.Paren, $"Expected {function.Arity} arguments but got {args.Count}.");
			}

			return function.Call(this, args);
		}

		public void VisitFunctionStmt(Function stmt)
		{
			FeeblFunction function = new FeeblFunction(stmt, environment);
			environment.Define(stmt.Name.Lexeme, function);
		}

		public void VisitReturnStmt(Return stmt)
		{
			object value = null;
			if (stmt.Value != null) value = Evaluate(stmt.Value);

			throw new ReturnException(value);
		}
	}
		
	[Serializable]
	public class RuntimeException : ApplicationException
	{
		public Token ErrorToken { get; }

		public RuntimeException() { }
		public RuntimeException(Token token, string message) : base(message)
		{
			ErrorToken = token;
		}
		public RuntimeException(string message) : base(message) { }
		public RuntimeException(string message, Exception inner) : base(message, inner) { }
		protected RuntimeException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class BreakException : RuntimeException { }

	[Serializable]
	public class ContinueException : RuntimeException { }

	[Serializable]
	public class ReturnException : RuntimeException 
	{
		public object Value { get; }

		public ReturnException(object value)
		{
			Value = value;
		}
	}
}
