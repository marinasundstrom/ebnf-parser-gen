using NUnit.Framework;
using System;
using Sundstrom.Ebnf;
using System.IO;

namespace Parser.Tests
{
	[TestFixture]
	public class EbnfParserTest
	{
        [Test]
        public void Tree()
        {
            var one = new Terminal("one");
            var two = new Terminal("two");
            var three = new Terminal("three");

            var root = new NonTerminal("root");
            root.Rule = one  | two | three;

            var grammar = new Grammar() { Root = root };
        }


        [Test]
		public void Parse ()
		{
			var ebnfParser = new EbnfParser ();
			var grammar = ebnfParser.Parse (@"grammar = ""foo"";");
		}

        [Test]
        public void ReadFile()
        {
            var ebnfParser = new EbnfParser();
            ebnfParser.Options.Root = "root";
            var grammar = ebnfParser.ReadFrom(
                File.OpenRead(GetAbsolutePath("Grammars\\test.g")));
        }

        private string GetAbsolutePath(string relativePath)
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(directory, relativePath);
        }
    }
}

