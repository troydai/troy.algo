using System;

namespace ScriptExecutor
{
	public class Program
	{
		public Program()
		{
		}

		public void Main(string[] args)
		{
			Console.WriteLine("Prototype: cross-platform script executor");

			var comSpec = Environment.GetEnvironmentVariable("ComSpec");
			Console.WriteLine("ComSpec: " + comSpec);
		}
	}
}