using System;
using Sundstrom.Ebnf;
using System.IO;
using Xunit;
using System.Reflection;

namespace Parser.Tests
{
	public class EbnfParserTest
	{
        [Fact]
        public void Tree()
        {
            var one = new Terminal("one");
            var two = new Terminal("two");
            var three = new Terminal("three");

            var root = new NonTerminal("root");
            root.Rule = one  | two | three;

            var grammar = new Grammar() { Root = root };
        }


        [Fact]
        public void Parse ()
		{
			var ebnfParser = new EbnfParser ();
			var grammar = ebnfParser.Parse (@"grammar = ""foo"";");
		}

        [Fact]
        public void ReadFile()
        {
            var ebnfParser = new EbnfParser();
            ebnfParser.Options.Root = "root";
            var grammar = ebnfParser.ReadFrom(
                File.OpenRead(GetAbsolutePath("Grammars\\test.g")));
        }

        private string GetAbsolutePath(string relativePath)
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            return Path.Combine(dirPath, relativePath);
        }
    }
}

