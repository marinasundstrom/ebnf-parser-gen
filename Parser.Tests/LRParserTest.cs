using System;
using Sundstrom.Parsing;
using Sundstrom.Grammars;
using Xunit;

namespace Parser.Tests
{
	public class LRParserTest
	{
		[Fact]
		public void TestCase ()
		{
			var grammar = new TestGrammar ();
		
			var realRoot = grammar.RealRoot;			
			
			var parser = new LRParser (grammar);

			var tree = parser.Parse ("foo 2");
		}
	}
}

