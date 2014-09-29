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
        NonTerminal option = new NonTerminal("option");
        NonTerminal inner = new NonTerminal("inner");
        NonTerminal root = new NonTerminal("root");

        public TestGrammar()
        {
            option.Rule = Term("1") | "2";
            inner.Rule = "foo" + option;
            root.Rule = "<" + inner + ">";

            Root = root;
        }
    }
}
