/*
 * Created by SharpDevelop.
 * User: Robert
 * Date: 2014-07-27
 * Time: 13:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Sundstrom.Ebnf;

namespace Sundstrom.Grammars
{
	/// <summary>
	/// Description of test.
	/// </summary>
	[GrammarInfo("Test", "Robert Sundström", "A basic test grammar.")]
	public class TestGrammar : Grammar
	{
		public TestGrammar()
		{
			var root = new NonTerminal("root");
			var option = new NonTerminal("option");
			
			option.Rule = Term("1") | "2";
			root.Rule = Term("Foo") | option;
			
			Root = root;
		}
	}
}
