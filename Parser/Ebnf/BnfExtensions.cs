using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sundstrom.Parsing;

namespace Sundstrom.Ebnf
{

    public static class BnfExtensions
    {
        public static Grammar ToBnf2(this Grammar grammar)
        {
            var nonTerminals = new List<NonTerminal>();

            var originalRoot = grammar.Root;
            var newGrammar = new Grammar();
            var newRoot = ProcessNt(originalRoot, ref nonTerminals);
            newGrammar.Root = newRoot;

            return newGrammar;
        }

        private static NonTerminal ProcessNt(NonTerminal originalNode, ref List<NonTerminal> nonTerminals)
        {
            var root = new NonTerminal(originalNode.Name);
            root.Rule = ProcessRule(originalNode.Rule, originalNode, ref nonTerminals);
            return null;
        }

        private static Expression ProcessRule(Expression rule, NonTerminal originalNode, ref List<NonTerminal> nonTerminals)
        {
            return null;
        }

        public static Grammar ToBnf(this Grammar grammar)
        {
            var nonTerminals = new List<NonTerminal>();

            var newGrammar = new Grammar();

            //foreach (var nonTerminal in grammar.NonTerminals.Except(new[] { grammar.RealRoot }))
            //{
            //    var alternations = nonTerminal.Rule
            //        .EnumerateAlternations().ToArray();

            //    foreach (var alternation in alternations)
            //    {
            //        var sequence = alternation.EnumerateConcatenated().ToArray();

            //    }
            //}

            return newGrammar;
        }
    }
}
