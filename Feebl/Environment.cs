using System.Collections.Generic;

namespace Feebl
{
	class Environment
	{
		public Environment Enclosing { get; }
		private Dictionary<string, object> values = new Dictionary<string, object>();

		public Environment() { }
		public Environment(Environment enclosing) => Enclosing = enclosing;

		public object Get(Token name)
		{
			if (values.ContainsKey(name.Lexeme))
			{
				return values[name.Lexeme];
			}

			if (Enclosing != null)
			{
				return Enclosing.Get(name);
			}

			throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
		}

		public void Define(string name, object obj)
		{
			values.Add(name, obj);
		}

		public void Assign(Token name, object value)
		{
			if (values.ContainsKey(name.Lexeme))
			{
				values[name.Lexeme] = value;
			}
			else
			{
				if (Enclosing != null)
				{
					Enclosing.Assign(name, value);
				}
				else
				{
					throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
				}
			}
		}
	}
}
