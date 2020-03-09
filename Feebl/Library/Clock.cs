using System.Collections.Generic;

namespace Feebl.Library
{
	class Clock : FeeblCallable
	{
		public int Arity => 0;

		public object Call(Interpreter interpreter, List<object> args)
		{
			return (double)System.Environment.TickCount;
		}

		public override string ToString() => "<native fn>";
	}

	class Exit : FeeblCallable
	{
		public int Arity => 0;

		public object Call(Interpreter interpreter, List<object> args)
		{
			System.Environment.Exit(0);
			return null;
		}
	}
}
