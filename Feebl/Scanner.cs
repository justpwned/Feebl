using System;
using System.Collections.Generic;

namespace Feebl
{
	class Scanner
	{
		private string source;
		private int start = 0;
		private int current = 0;
		private int line = 1;
		private List<Token> tokens = new List<Token>();

		private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
		{
			{ "class",    TokenType.CLASS },
			{ "else",     TokenType.ELSE },
			{ "false",    TokenType.FALSE },
			{ "for",      TokenType.FOR },
			{ "func",     TokenType.FUNC },
			{ "if",       TokenType.IF },
			{ "nil",      TokenType.NIL },
			{ "return",   TokenType.RETURN },
			{ "super",    TokenType.SUPER },
			{ "this",     TokenType.THIS },
			{ "true",     TokenType.TRUE },
			{ "let",      TokenType.LET },
			{ "while",    TokenType.WHILE },
			{ "break",    TokenType.BREAK },
			{ "continue", TokenType.CONTINUE }
		};

		public Scanner(string source) => this.source = source;

		public List<Token> ScanTokens()
		{
			while (!IsAtEnd())
			{
				start = current;
				ScanToken();
			}

			tokens.Add(new Token(TokenType.EOF, "", null, line));
			return tokens;
		}

		private void ScanToken()
		{
			char c = Advance();
			switch (c)
			{
				case '(': AddToken(TokenType.LEFT_PAREN); break;
				case ')': AddToken(TokenType.RIGHT_PAREN); break;
				case '{': AddToken(TokenType.LEFT_BRACE); break;
				case '}': AddToken(TokenType.RIGHT_BRACE); break;
				case ',': AddToken(TokenType.COMMA); break;
				case '.': AddToken(TokenType.DOT); break;
				case '-': AddToken(TokenType.MINUS); break;
				case '+': AddToken(TokenType.PLUS); break;
				case ';': AddToken(TokenType.SEMICOLON); break;
				case '*': AddToken(TokenType.STAR); break;
				case '^': AddToken(TokenType.BIT_EX_OR); break;
				case '%': AddToken(TokenType.PERCENT); break;
				case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
				case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
				case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
				case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
				case '|': AddToken(Match('|') ? TokenType.LOG_OR : TokenType.BIT_INC_OR); break;
				case '&': AddToken(Match('&') ? TokenType.LOG_AND : TokenType.BIT_AND); break;
				case '/':
				{
					if (Match('/'))
					{
						while (Peek() != '\n' && !IsAtEnd()) Advance();
					}
					else
					{
						AddToken(TokenType.SLASH);
					}
				} break;
				case ' ':
				case '\r':
				case '\t': break;
				case '"': ScanString(); break;
				case '\n': ++line; break;
				default:
				{
					if (IsDigit(c))
					{
						ScanNumber();
					}
					else if (IsAlpha(c))
					{
						ScanIdentifier();
					}
					else
					{
						Feebl.Error(line, $"Unexpected character: {c}.");
					}
				} break;
			}
		}

		private bool IsDigit(char c) => c >= '0' && c <= '9';
		private bool IsAlpha(char c) => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_';
		private bool IsAlphaNum(char c) => IsDigit(c) || IsAlpha(c);

		private bool IsAtEnd() => current >= source.Length;
		private char Advance() => source[current++];
		private char Peek() => IsAtEnd() ? '\0' : source[current];
		private char PeekNext() => current + 1 >= source.Length ? '\0' : source[current + 1];

		private void AddToken(TokenType type) => AddToken(type, null);
		private void AddToken(TokenType type, object literal)
		{
			string text = source.Substring(start, current - start);
			tokens.Add(new Token(type, text, literal, line));
		}

		private bool Match(char expected)
		{
			if (IsAtEnd() || source[current] != expected)
			{
				return false;
			}
			++current;
			return true;
		}

		private void ScanString()
		{
			while (Peek() != '"' && !IsAtEnd())
			{
				if (Peek() == '\n') ++line;
				Advance();
			}

			if (IsAtEnd())
			{
				Feebl.Error(line, "Unterminated string.");
			}
			else
			{
				Advance();
				string value = source.Substring(start + 1, current - 1 - (start + 1));
				AddToken(TokenType.STRING, value);
			}
		}

		private void ScanNumber()
		{
			while (IsDigit(Peek())) Advance();
			
			if (Peek() == '.' && IsDigit(PeekNext()))
			{
				Advance();
				while (IsDigit(Peek())) Advance();
			}

			double value = Double.Parse(source.Substring(start, current - start));
			AddToken(TokenType.NUMBER, value);
		}

		private void ScanIdentifier()
		{
			while (IsAlphaNum(Peek())) Advance();

			string text = source.Substring(start, current - start);
			TokenType type = keywords.ContainsKey(text) ? keywords[text] : TokenType.IDENTIFIER;
			AddToken(type);
		}
	}
}
