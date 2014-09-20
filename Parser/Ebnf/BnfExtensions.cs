using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sundstrom.Parsing;

namespace Sundstrom.Ebnf
{
    public static class SequenceExtensions
    {
        public static IEnumerable<Expression> EnumerateConcatenated(this Expression node)
        {
            var concatenation = node as Concatenation;
            if (concatenation != null)
            {
                yield return concatenation.Left;
                var result = EnumerateConcatenated(
                    concatenation.Right);
                foreach (var r in result)
                {
                    yield return r;
                }
            }
            else
            {
                yield return node;
            }
        }

        public static IEnumerable<Expression> EnumerateAlternations(this Expression node)
        {
            var alternation = node as Alternation;
            if (alternation != null)
            {
                yield return alternation.Left;
                var result = EnumerateAlternations(
                    alternation.Right);
                foreach (var r in result)
                {
                    yield return r;
                }
            }
            else
            {
                yield return node;
            }
        }
    }

    public static class BnfExtensions
    {
        public static Grammar ToBnf(this Grammar grammar)
        {
            var nonTerminals = new List<NonTerminal>();

            var originalRoot = grammar.Root;
            var newGrammar = new Grammar();
            var newRoot = ProcessNt(nonTerminals, originalRoot);
            newGrammar.Root = newRoot;

            return newGrammar;
        }

        private static NonTerminal ProcessNt(List<NonTerminal> nonTerminals, NonTerminal nonTerminal)
        {
            var nt = nonTerminals.FirstOrDefault(i => i.Name == nonTerminal.Name);
            if (nt == null)
            {
                nt = new NonTerminal(nonTerminal.Name);
                var rule = ProcessExpression(nonTerminals, nonTerminal.Rule);
                nt.Rule = rule;
                nonTerminals.Add(nt);
            }
            return nt;
        }

        private static Expression ProcessRule(List<NonTerminal> nonTerminals, Expression rule)
        {
            var queue = rule
                .EnumerateConcatenated()
                .ToReverseQueue();

            Concatenation concatenation = null;
            while (queue.Count > 0)
            {
                var right = queue.Dequeue();
                if (concatenation == null)
                {
                    var left = queue.Dequeue();
                    concatenation = new Concatenation(left, right);
                }
                else
                {
                    concatenation = new Concatenation(right, concatenation);
                }
            }
            return concatenation;
        }

        private static Expression ProcessExpression(List<NonTerminal> nonTerminals, Expression expr)
        {
            var terminal = expr as Terminal;
            if (terminal != null)
            {
                return new Terminal(terminal.Value);
            }
            else
            {
                var nonTerminal = expr as NonTerminal;
                if (nonTerminal != null)
                {
                    return ProcessNt(nonTerminals, nonTerminal);
                }
                else
                {
                    var concatenation = expr as Concatenation;
                    if (concatenation != null)
                    {
                        return ParseConcatenation(nonTerminals, concatenation);
                    }
                    else
                    {
                        var alternation = expr as Alternation;
                        if (alternation != null)
                        {
                            return ParseAlternation(nonTerminals, alternation);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }
        }

        private static Expression ParseConcatenation(List<NonTerminal> nonTerminals, Concatenation concatenation)
        {
            var left = ProcessExpression(nonTerminals, concatenation.Left);
            var right = ProcessExpression(nonTerminals, concatenation.Right);
            var newConcatenation = new Concatenation(left, right);
            return newConcatenation;
        }

        private static Expression ParseAlternation(List<NonTerminal> nonTerminals, Alternation alternation)
        {
            //var left = ProcessExpression(nonTerminals, alternation.Left);
            //var right = ProcessExpression(nonTerminals, alternation.Right);
            //var newAlternation = new Alternation(left, right);
            //return newAlternation;

            var alternations = alternation.EnumerateAlternations().ToArray();

            //var items1 = alternations.Where()

            return null;
        }
    }
}
