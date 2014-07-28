using NUnit.Framework;
using System;
using Sundstrom.Parsing;
using Sundstrom.Grammars;

namespace Parser.Tests
{
	[TestFixture]
	public class LRParserTest
	{
		[Test]
		public void TestCase ()
		{
			var grammar = new TestGrammar ();
		
			var realRoot = grammar.RealRoot;			
			
			var parser = new LRParser (grammar);

			var tree = parser.Parse ("foo 2");
		}
	}
}

