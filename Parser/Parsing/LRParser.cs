using Sundstrom.Ebnf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sundstrom.Parsing
{
    public class LRParser
    {
        private Sundstrom.Ebnf.Grammar grammar;
        private List<ParserState> States = new List<ParserState>();

        public LRParser(Sundstrom.Ebnf.Grammar grammar)
        {
            this.grammar = grammar;
        }

        public object Parse(string v)
        {
            States.Clear();

            CreateStates();

            return null;
        }

        private void CreateStates()
        {
            States.Add(new ParserState(grammar, null, null, true));
            States.Add(new ParserState(grammar, null, null, false));

            foreach (var nonTerminal in grammar.NonTerminals)
            {
                if (!HasEntryState(nonTerminal))
                {
                    CreateEntryState(grammar, nonTerminal);
                }
            }

            for (int i = 0; i < States.Count; i++)
            {
                var state = States[i];
                if (state.IsInitial)
                {
                    ParseStates(state);
                }
            }
        }

        private void ParseStates(ParserState state)
        {
            if (state.IsInitial && state.Production == null && state.Terminal == null)
            {
                var root = States.Find(x => x.IsRoot);
                state.States.Add(root);
            }
            else if (!state.IsInitial && state.Production == null && state.Terminal == null)
            {
                return;
            }
            else
            {
                var lastState = state;
                ParserState state2 = state;
                var sequence = state.Production.Rule.AsEnumerable().ToArray();
                foreach (var x in sequence.Skip(1))
                {
                    var nonTerminal = x as NonTerminal;
                    if (nonTerminal != null)
                    {
                        var result = States.Where(x2 => x2.Production == nonTerminal && x2.IsInitial);
                        foreach (var state3 in result)
                        {
                            lastState.States.Add(state3);
                            //state2 = state3.FindOuter();
                        }
                    }
                    else
                    {
                        state2 = new ParserState(grammar, (Terminal)x, state.Production, false);
                        States.Add(state2);
                    }
                    lastState = state2;
                }
            }
        }

        private bool HasEntryState(NonTerminal nonTerminal)
        {
            return States.Any(x => x.Production == nonTerminal && x.IsInitial);
        }

        private ParserState CreateEntryState(Grammar grammar, NonTerminal nonTerminal)
        {
            var tokens = nonTerminal.Rule.AsEnumerable().ToArray();
            var first = tokens.First();

            var alternation = first as Alternation;
            if (alternation != null)
            {
                foreach (var route in alternation.Alternations())
                {
                    var tokens2 = route.AsEnumerable().ToArray();
                    var first2 = tokens2.First();
                    var state = new ParserState(grammar, (Terminal)first2, nonTerminal, true, alternation);
                    States.Add(state);
                    return state;
                }
            }
            else
            {
                var nonTerminal2 = first as NonTerminal;
                if (nonTerminal2 != null)
                {
                    if (!HasEntryState(nonTerminal2))
                    {
                        return CreateEntryState(grammar, nonTerminal2);
                    }
                }
                else
                {
                    var state = new ParserState(grammar, (Terminal)first, nonTerminal, true);
                    States.Add(state);
                    return state;
                }
            }

            throw new Exception();
        }
    }

    public class ParserState
    {
        public ParserState(
            Grammar grammar,
            Terminal token,
            NonTerminal rule,
            bool isInitial = false,
            Alternation alternation = null
        )
        {
            Grammar = grammar;
            Terminal = token;
            Production = rule;
            IsInitial = isInitial;
            Alternation = alternation;

            States = new List<ParserState>();
        }

        public bool IsRoot
        {
            get
            {
                return Production == Grammar.Root;
            }
        }

        public Grammar Grammar { get; private set; }

        public Terminal Terminal { get; private set; }

        public Alternation Alternation { get; private set; }

        public NonTerminal Production { get; private set; }

        public bool IsInitial { get; private set; }

        public bool IsReady { get; set; }

        public List<ParserState> States { get; private set; }
    }
}
