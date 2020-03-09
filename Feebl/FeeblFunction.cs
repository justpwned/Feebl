using System;
using System.Collections.Generic;

namespace Feebl
{
	class FeeblFunction : FeeblCallable
	{
		public int Arity => declaration.Params.Count;
		
		private Function declaration;
		private Environment closure;

		public FeeblFunction(Function declaration, Environment closure)
		{
			this.declaration = declaration;
			this.closure = closure;
		}

		public object Call(Interpreter interpreter, List<object> args)
		{
			Environment environment = new Environment(closure);
			for (int i = 0; i < declaration.Params.Count; ++i)
			{
				environment.Define(declaration.Params[i].Lexeme, args[i]);
			}

			try
			{
				interpreter.ExecuteBlock(((Block)declaration.Body).Statements, environment);
			}
			catch (ReturnException e)
			{
				return e.Value;
			}

			return null;
		}

		public override string ToString() => $"<fn {declaration.Name.Lexeme}>";
	}
}
