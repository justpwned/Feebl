using System;
using System.IO;
using System.Collections.Generic;

// Feebl from "Feeble" - "Weak and without energy, strength, or power", 
//						 "Not effective or good"
//                       "Not good enough to achieve the intended result"

// Thereby showing original intentions of creating that interpreter


namespace Feebl
{
    class Feebl
    {
		private static Interpreter interpreter = new Interpreter();

		private static bool hadError = false;
		private static bool hadRuntimeError = false;

        static int Main(string[] args)
        {
			if (args.Length > 1)
			{
				Console.WriteLine("Usage: Feebl.exe [script]");
				return 1;
			}
			else if (args.Length == 1)
			{
				RunFile(args[0]);
			}
			else
			{
				RunPrompt();
			}

			return 0;
        }

		private static void RunFile(string filename)
		{
			if (File.Exists(filename))
			{
				string fileContents = File.ReadAllText(filename);

				Scanner scanner = new Scanner(fileContents);
				List<Token> tokens = scanner.ScanTokens();
				Parser parser = new Parser(tokens);
				List<Stmt> ast = parser.ParseFile();

				if (hadError) System.Environment.Exit(1);

				interpreter.Interpret(ast);

				if (hadRuntimeError) System.Environment.Exit(2);
			}
			else
			{
				Console.WriteLine("File \"{0}\" doesn't exist!", filename);
			}
		}

		private static void RunPrompt()
		{
			while (true)
			{
				hadError = false;
				Console.Write("> ");

				string source = Console.ReadLine();
				Scanner scanner = new Scanner(source);
				List<Token> tokens = scanner.ScanTokens();
				Parser parser = new Parser(tokens);	
				object ast = parser.ParseRepl();
				
				if (hadError) continue;

				if (ast is List<Stmt>)
				{
					if (hadError) continue;

					interpreter.Interpret((List<Stmt>)ast);
				}
				else if (ast is Expr)
				{
					string result = interpreter.Interpret((Expr)ast);
					if (result != null)
					{
						Console.WriteLine(result);
					}
				}
			}
		}

		private static void Report(int line, string where, string msg)
		{
			Console.WriteLine("[line {0}] Error{1}: {2}", line, where, msg);
			hadError = true;
		}

		public static void Error(int line, string msg)
		{
			Report(line, "", msg);
		}

		public static void Error(Token token, string msg)
		{
			if (token.Type == TokenType.EOF)
			{
				Report(token.Line, " at end", msg);
			}
			else
			{
				Report(token.Line, " at '" + token.Lexeme + "'", msg);
			}
		}

		public static void RuntimeError(RuntimeException e)
		{
			Console.WriteLine($"[line {e.ErrorToken.Line}] {e.Message}");
			hadRuntimeError = true;
		}
    }
}
