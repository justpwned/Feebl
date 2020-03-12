namespace Feebl
{
	public enum TokenType
	{
		LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
		COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,
		COLON,

		BANG, BANG_EQUAL,
		EQUAL, EQUAL_EQUAL,
		GREATER, GREATER_EQUAL,
		LESS, LESS_EQUAL,

		IDENTIFIER, STRING, NUMBER,

		BIT_AND, BIT_INC_OR, BIT_EX_OR, LOG_AND, LOG_OR,
		CLASS, ELSE, FALSE, FUNC, FOR, IF, NIL, RETURN, 
		SUPER, THIS, TRUE, LET, WHILE, BREAK, CONTINUE, 
		PERCENT, INCREMENT, DECREMENT,

		EOF
	}

	public class Token
	{
		public TokenType Type { get; }
		public string Lexeme { get; }
		public object Literal { get; }
		public int Line { get; }

		public Token(TokenType type, string lexeme, object literal, int line)
		{
			Type = type;
			Lexeme = lexeme;
			Literal = literal;
			Line = line;
		}

		public override string ToString() => $"{Type} {Lexeme}{Literal}";
	}
}
