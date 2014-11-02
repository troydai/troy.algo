using System;
using System.Diagnostics;
using System.Linq;

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

			if (args.Length != 1)
			{
				Console.WriteLine("Usage: k run [single script block to execute]");
				return;
			}

			var arguments = args[0].Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries)
								   .Select(one => one.Trim())
								   .ToArray();

			Console.WriteLine("Is Mono: " + PlatformHelper.IsMono);
			if (PlatformHelper.IsMono)
			{
				arguments[0] = arguments[0] + ".sh";
			}
			else
			{
				var comSpec = Environment.GetEnvironmentVariable("ComSpec");
				Console.WriteLine("ComSpec: " + comSpec);

				if (!string.IsNullOrEmpty(comSpec))
				{
					arguments = new[] { comSpec, "/C", "\""}
						.Concat(arguments)
						.Concat(new[] {"\""})
						.ToArray();
				}
			}

			Console.WriteLine("Command: {0}", string.Join(" ", arguments));
			var startInfo = new ProcessStartInfo
			{
				FileName = arguments.FirstOrDefault(),
				Arguments = string.Join(" ", arguments.Skip(1)),
				UseShellExecute = false
			};

			var process = Process.Start(startInfo);
			process.WaitForExit();
		}
	}
}