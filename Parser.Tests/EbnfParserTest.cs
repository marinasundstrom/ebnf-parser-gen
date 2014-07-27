using NUnit.Framework;
using System;
using Sundstrom.Ebnf;

namespace Parser.Tests
{
	[TestFixture]
	public class EbnfParserTest
	{
		[Test]
		public void Simple ()
		{
			var ebnfParser = new EbnfParser ();
			var grammar = ebnfParser.Parse ("grammar = \"" + "foo" + "\";");

			Assert.IsTrue (grammar.Root.Rule is Terminal);

		}
	}
}

