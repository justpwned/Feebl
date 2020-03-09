using System.Collections.Generic;

namespace Feebl
{
	interface FeeblCallable
	{
		int Arity { get; }
		object Call(Interpreter interpreter, List<object> args);
	}
}
