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
		private static bool debugMode = false;

        static int Main(string[] args)
        {
			if (args.Length == 0)
			{
				RunPrompt();
			}
			else if (args.Length == 1)
			{
				if (args[0][0] == '-')
				{
					if (args[0] == "-d")
					{
						debugMode = true;
					}
					else
					{
						Console.WriteLine("Unknown interpreter switch: {0}", args[0]);
						return 1;
					}
					RunPrompt();
				}
				else
				{
					RunFile(args[0]);
				}
			}
			else if (args.Length == 2)
			{
				if (args[1] == "-d")
				{
					debugMode = true;
				}
				else
				{
					Console.WriteLine("Unknown interpreter switch: {0}", args[1]);
				}

				RunFile(args[0]);
			}
			else
			{
				Console.WriteLine("Usage: Feebl.exe [script] [-d]");
				return 1;
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

				if (debugMode)
				{
					AstPrinter astp = new AstPrinter();
					foreach (var stmt in ast)
					{
						Console.WriteLine(astp.Print(stmt));
					}
					Console.WriteLine();
				}

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
			if (!debugMode)
			{
				Console.WriteLine("Feebl v.0.1.");
			}
			else
			{
				Console.WriteLine("Feebl v.0.1. AST print mode on.");
			}


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

					if (debugMode)
					{
						AstPrinter astp = new AstPrinter();
						foreach (var stmt in (ast as List<Stmt>))
						{
							Console.WriteLine(astp.Print(stmt));
						}
					}

					interpreter.Interpret((List<Stmt>)ast);
				}
				else if (ast is Expr)
				{
					if (debugMode)
					{
						AstPrinter astp = new AstPrinter();
						Console.WriteLine(astp.Print(ast as Expr));
					}

					string result = interpreter.Interpret((Expr)ast);
					if (result != null)
					{
						Console.WriteLine("-> {0}", result);
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
