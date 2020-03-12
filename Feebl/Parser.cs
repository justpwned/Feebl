using System;
using System.Collections.Generic;

namespace Feebl
{
	class Parser
	{
		private List<Token> tokens;
		private int current = 0;

		private bool allowExpression;
		private bool foundExpression = false;

		private int loopDepth = 0;

		public Parser(List<Token> tokens)
		{
			this.tokens = tokens;
		}

		private bool Match(params TokenType[] types)
		{
			foreach (var type in types)
			{
				if (Check(type))
				{
					Advance();
					return true;
				}
			}
			return false;
		}

		private bool Check(TokenType type) => IsAtEnd() ? false : Peek().Type == type;
		private bool IsAtEnd() => Peek().Type == TokenType.EOF;
		private Token Peek() => tokens[current];
		private Token Previous() => tokens[current - 1];
		private Token Advance() => IsAtEnd() ? tokens[current - 1] : tokens[current++];

		public List<Stmt> ParseFile()
		{
			List<Stmt> statements = new List<Stmt>();
			while (!IsAtEnd())
			{
				statements.Add(ParseDeclaration());
			}
			return statements;
		}

		public object ParseRepl()
		{
			allowExpression = true;
			List<Stmt> statements = new List<Stmt>();
			while (!IsAtEnd())
			{
				statements.Add(ParseDeclaration());
				if (foundExpression)
				{
					Stmt last = statements[statements.Count - 1];
					return ((Expression)last).Expr;
				}
				allowExpression = false;
			}

			return statements;
		}

		private Stmt ParseDeclaration()
		{
			try
			{
				if (Match(TokenType.FUNC)) return ParseFunction("function");
				if (Match(TokenType.LET)) return ParseVarDeclaration();
				return ParseStatement();
			}
			catch (ParseException e)
			{
				Sync();
				return null;
			}
		}

		private Stmt ParseFunction(string kind)
		{
			Token name = Consume(TokenType.IDENTIFIER, $"Expected {kind} name.");
			Consume(TokenType.LEFT_PAREN, $"Expected '(' after {kind} name.");
			List<Token> parameters = new List<Token>();

			if (!Check(TokenType.RIGHT_PAREN))
			{
				do
				{
					if (parameters.Count >= 255)
					{
						Error(Peek(), "Cannot have more than 255 parameters.");
					}
					parameters.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name."));
				} while (Match(TokenType.COMMA));
			}
			Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameters.");

			Consume(TokenType.LEFT_BRACE, $"Expected '{{' before {kind} body.");
			Block body= ParseBlock() as Block;

			return new Function(name, parameters, body);
		}

		private Stmt ParseVarDeclaration()
		{
			Token name = Consume(TokenType.IDENTIFIER, "Expected a variable name.");

			Expr initializer = null;
			if (Match(TokenType.EQUAL))
			{
				initializer = ParseExpression();
			}

			Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration.");

			return new Var(name, initializer);
		}

		private Stmt ParseStatement()
		{
			if (Match(TokenType.RETURN)) return ParseReturnStatement();
			if (Match(TokenType.FOR)) return ParseForStatement();
			if (Match(TokenType.WHILE)) return ParseWhileStatement();
			if (Match(TokenType.BREAK)) return ParseBreakStatement();
			if (Match(TokenType.CONTINUE)) return ParseContinueStatement();
			if (Match(TokenType.IF)) return ParseIfStatement();
			if (Match(TokenType.LEFT_BRACE)) return ParseBlock();
			return ParseExpressionStatement();
		}

		private Stmt ParseReturnStatement()
		{
			Token keyword = Previous();
			Expr value = null;
			if (!Check(TokenType.SEMICOLON))
			{
				value = ParseExpression();
			}

			Consume(TokenType.SEMICOLON, "Expected ';' after return value.");
			return new Return(keyword, value);
		}

		private Stmt ParseBreakStatement()
		{
			if (loopDepth == 0)
			{
				Error(Previous(), "Must be inside a loop to use 'break'.");
			}
			Consume(TokenType.SEMICOLON, "Expected ';' after 'break'.");
			return new Break();
		}

		private Stmt ParseContinueStatement()
		{
			if (loopDepth == 0)
			{
				Error(Previous(), "Must be inside a loop to use 'continue'.");
			}
			Consume(TokenType.SEMICOLON, "Expected ';' after 'continue'.");
			return new Continue();
		}

		private Stmt ParseForStatement()
		{
			Consume(TokenType.LEFT_PAREN, "Expected '(' after 'for'.");

			Stmt initializer;
			if (Match(TokenType.SEMICOLON))
			{
				initializer = null;
			}
			else if (Match(TokenType.LET))
			{
				initializer = ParseVarDeclaration();
			}
			else
			{
				initializer = ParseExpressionStatement();
			}

			Expr cond = null;
			if (!Check(TokenType.SEMICOLON))
			{
				cond = ParseExpression();
			}
			Consume(TokenType.SEMICOLON, "Expected ';' after loop condition.");

			Expr inc = null;
			if (!Check(TokenType.RIGHT_PAREN))
			{
				inc = ParseExpression();
			}
			Consume(TokenType.RIGHT_PAREN, "Expected ')' after for clauses");

			try
			{
				++loopDepth;
				Stmt body = ParseStatement();

				Stmt forStmt = new For(cond == null ? new Literal(true) : cond,
									   inc != null ? new Expression(inc) : null, 
									   body);

				List<Stmt> statements = new List<Stmt>();
				if (initializer != null)
				{
					statements.Add(initializer);
				}
				statements.Add(forStmt);

				return new Block(statements);
			}
			finally
			{
				--loopDepth;
			}
		}

		private Stmt ParseWhileStatement()
		{
			Consume(TokenType.LEFT_PAREN, "Expected '(' after 'while'.");
			Expr cond = ParseExpression();
			Consume(TokenType.RIGHT_PAREN, "Expected ')' after condition.");

			try
			{
				++loopDepth;
				Stmt body = ParseStatement();
				return new While(cond, body);
			}
			finally
			{
				--loopDepth;
			}
		}

		private Stmt ParseIfStatement()
		{
			Consume(TokenType.LEFT_PAREN, "Expected '(' after 'if'.");
			Expr cond = ParseExpression();
			Consume(TokenType.RIGHT_PAREN, "Expected ')' after if condition.");

			Stmt thenBranch = ParseStatement();
			Stmt elseBranch = null;
			if (Match(TokenType.ELSE))
			{
				elseBranch = ParseStatement();
			}

			return new If(cond, thenBranch, elseBranch);
		}

		private Stmt ParseBlock()
		{
			List<Stmt> statements = new List<Stmt>();
			while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
			{
				statements.Add(ParseDeclaration());
			}

			Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
			return new Block(statements);
		}

		private Stmt ParseExpressionStatement()
		{
			Expr expr = ParseExpression();

			if (allowExpression && IsAtEnd())
			{
				foundExpression = true;
			}
			else
			{
				Consume(TokenType.SEMICOLON, "Expected ';' after expression.");
			}

			return new Expression(expr);
		}

		private Expr ParseExpression()
		{
			return ParseAssignment();
		}

		private Expr ParseAssignment()
		{
			Expr expr = ParseLogOr();

			if (Match(TokenType.EQUAL))
			{
				Token equals = Previous();
				Expr value = ParseAssignment();

				if (expr is Variable)
				{
					Token name = ((Variable)expr).Name;
					return new Assign(name, value);
				}

				Error(equals, "Invalid assignment target.");
			}

			return expr;
		}

		private Expr ParseLogOr()
		{
			Expr expr = ParseLogAnd();

			while (Match(TokenType.LOG_OR))
			{
				Token op = Previous();
				Expr right = ParseLogAnd();
				expr = new Logical(expr, op, right);
			}

			return expr;
		}

		private Expr ParseLogAnd()
		{
			Expr expr = ParseBitIncOr();

			while (Match(TokenType.LOG_AND))
			{
				Token op = Previous();
				Expr right = ParseBitIncOr();
				expr = new Binary(expr, op, right);
			}

			return expr;
		}

		private Expr ParseBitIncOr()
		{
			Expr expr = ParseBitExOr();

			while (Match(TokenType.BIT_INC_OR))
			{
				Token op = Previous();
				Expr right = ParseBitExOr();
				expr = new Binary(expr, op, right);
			}

			return expr;
		}

		private Expr ParseBitExOr()
		{
			Expr expr = ParseBitAnd();

			while (Match(TokenType.BIT_EX_OR))
			{
				Token op = Previous();
				Expr right = ParseBitAnd();
				expr = new Binary(expr, op, right);
			}

			return expr;
		}

		private Expr ParseBitAnd()
		{
			Expr expr = ParseEquality();

			while (Match(TokenType.BIT_AND))
			{
				Token op = Previous();
				Expr right = ParseEquality();
				expr = new Binary(expr, op, right);
			}

			return expr;
		}

		private Expr ParseEquality()
		{
			Expr expr = ParseComparison();
			while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
			{
				Token op = Previous();
				Expr right = ParseComparison();
				expr = new Binary(expr, op, right);
			}
			return expr;
		}

		private Expr ParseComparison()
		{
			Expr expr = ParseAddition();
			while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
			{
				Token op = Previous();
				Expr right = ParseAddition();
				expr = new Binary(expr, op, right);
			}
			return expr;
		}

		private Expr ParseAddition()
		{
			Expr expr = ParseMultiplication();
			while (Match(TokenType.PLUS, TokenType.MINUS))
			{
				Token op = Previous();
				Expr right = ParseMultiplication();
				expr = new Binary(expr, op, right);
			}
			return expr;
		}

		private Expr ParseMultiplication()
		{
			Expr expr = ParseUnary();
			while (Match(TokenType.SLASH, TokenType.STAR, TokenType.PERCENT))
			{
				Token op = Previous();
				Expr right = ParseUnary();
				expr = new Binary(expr, op, right);
			}
			return expr;
		}

		private Expr ParseUnary()
		{
			if (Match(TokenType.BANG, TokenType.MINUS, TokenType.PLUS))
			{
				Token op = Previous();
				Expr right = ParseUnary();
				return new Unary(op, right);
			}
			return ParseCall();
		}

		private Expr ParseCall()
		{
			Expr expr = ParsePrimary();
			while (true)
			{
				if (!Match(TokenType.LEFT_PAREN)) break;

				expr = ParseFinishCall(expr);
			}
			return expr;
		}

		private Expr ParseFinishCall(Expr callee)
		{
			List<Expr> args = new List<Expr>();
			if (!Check(TokenType.RIGHT_PAREN))
			{
				do
				{
					if (args.Count >= 255)
					{
						Error(Peek(), "Cannot have more than 255 arguments.");
					}
					args.Add(ParseExpression());
				} while (Match(TokenType.COMMA));
			}

			Token paren = Consume(TokenType.RIGHT_PAREN, "Expected ')' after arguments.");

			return new Call(callee, paren, args);
		}

		private Expr ParsePrimary()
		{
			if (Match(TokenType.FALSE)) return new Literal(false);
			if (Match(TokenType.TRUE)) return new Literal(true);
			if (Match(TokenType.NIL)) return new Literal(null);
			if (Match(TokenType.NUMBER, TokenType.STRING)) return new Literal(Previous().Literal);
			if (Match(TokenType.IDENTIFIER)) return new Variable(Previous());
			if (Match(TokenType.LEFT_PAREN))
			{
				Expr expr = ParseExpression();
				Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
				return new Grouping(expr);
			}
			throw Error(Peek(), "Expected expression.");
		}

		// Error handling methods

		private Token Consume(TokenType type, string msg)
		{
			if (Check(type)) return Advance();
			throw Error(Peek(), msg);
		}

		private ParseException Error(Token token, string msg)
		{
			Feebl.Error(token, msg);
			return new ParseException();
		}

		private void Sync()
		{
			Advance();
			while (!IsAtEnd())
			{
				TokenType t = Peek().Type;
				if (Previous().Type == TokenType.SEMICOLON || t == TokenType.CLASS ||
					t == TokenType.FUNC || t == TokenType.LET || t == TokenType.FOR ||
					t == TokenType.IF || t == TokenType.WHILE || t == TokenType.RETURN) 
						return;
				Advance();
			}
		}
	}

	[Serializable]
	public class ParseException : ApplicationException
	{
		public ParseException() { }
		public ParseException(string message) : base(message) { }
		public ParseException(string message, Exception inner) : base(message, inner) { }
		protected ParseException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}