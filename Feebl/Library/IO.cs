using System;
using System.Collections.Generic;

namespace Feebl.Library.IO
{
	class Print : FeeblCallable
	{
		public int Arity { get; set; }

		public object Call(Interpreter interpreter, List<object> args)
		{
			if (args.Count > 0) Console.Write(args[0]);
			for (int i = 1; i < args.Count; ++i)
			{
				Console.Write($" {args[i]}");
			}
			return null;
		}

		public override string ToString() => "<native fn print(args...)>";
	}

	class Println : FeeblCallable
	{
		public int Arity { get; set; }

		public object Call(Interpreter interpreter, List<object> args)
		{
			if (args.Count > 0) Console.Write(args[0]);
			for (int i = 1; i < args.Count; ++i)
			{
				Console.Write($" {args[i]}");
			}
			Console.WriteLine();
			return null;
		}

		public override string ToString() => "<native fn println(args...)>";
	}

	class Read : FeeblCallable
	{
		public int Arity { get; } = 0;

		public object Call(Interpreter interpreter, List<object> args)
		{
			string input = Console.ReadLine();
			bool success = Double.TryParse(input, out double number);
			if (success)
			{
				return number;
			}
			else
			{
				return input;
			}
		}

		public override string ToString() => "<native fn read()>";
	}
}
