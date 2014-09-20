/*
 * Created by SharpDevelop.
 * User: Robert
 * Date: 2014-07-28
 * Time: 15:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sundstrom.Ebnf;
namespace Sundstrom.Parsing
{
    /// <summary>
    /// Description of GrammarStateParser.
    /// </summary>
    public sealed class GrammarStateParser
    {
        public GrammarStateParser(Grammar grammar)
        {
            Grammar = grammar;
        }

        public Grammar Grammar { get; private set; }

        public List<ParserState> States { get; private set; }

        private List<NonTerminalTransitionState> NonTerminalTransitionStates { get; set; }

        public ParserState LastState { get; private set; }

        public ParserState ParseStates()
        {
            States = new List<ParserState>();
            NonTerminalTransitionStates = new List<NonTerminalTransitionState>();

            var root = Grammar.Root;

            var initialState = CreateState();

            // Create states

            ParseNonTerminalWithRule(root);

            return initialState;
        }

        private void ParseNonTerminalWithRule(NonTerminal currentNonTerminal)
        {
            
        }

        ParserState CreateState()
        {
            LastState = States.LastOrDefault();
            var state = new ParserState(string.Format("S{0}", States.Count));
            States.Add(state);
            return state;
        }
    }

    public class NonTerminalTransitionState
    {
        public NonTerminalTransitionState(
            NonTerminal nonTerminal,
            Expression expression,
            ParserState state)
        {
            NonTerminal = nonTerminal;
            Expression = expression;
            State = state;
        }

        public NonTerminal NonTerminal { get; private set; }

        public Expression Expression { get; private set; }

        public ParserState State { get; private set; }
    }

    public class ParserState
    {
        public ParserState(string name)
        {
            Name = name;
            ActionTable = new ActionTable();
        }

        public string Name { get; private set; }

        public ActionTable ActionTable { get; private set; }

        public NonTerminal NonTerminal { get; internal set; }

        internal void SetNonTerminal(NonTerminal value)
        {
            NonTerminal = value;
        }

        public string ToListFormString()
        {
            return string.Format("{0}:\n{1}", Name, ActionTable.ToListFormString());
        }

        public override string ToString()
        {
            if (ActionTable.All(x => x.Action != ActionTableAction.Transition))
                return Name;

            return string.Format("({0}: {1})", Name, ActionTable);
        }
    }

    public class ActionTable : IEnumerable<ActionTableEntry>
    {

        private List<ActionTableEntry> items = new List<ActionTableEntry>();

        public ActionTable()
        {

        }

        internal void AddTransition(Expression from, ParserState to)
        {
            items.Add(new ActionTableEntry.ActionTableTransitionEntry(from, to));
        }

        internal void AddReduce(string to, Expression from)
        {
            items.Add(new ActionTableEntry.ActionTableReduceEntry(to, from));
        }

        public string ToCompressedFormString()
        {
            var sb = new StringBuilder();
            var transitions = this.Where(x => x.Action == ActionTableAction.Transition);
            foreach (var action in transitions.ToArray())
            {
                sb.Append(action.ToString());
                if (action != transitions.Last())
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }

        public string ToListFormString()
        {
            var sb = new StringBuilder();
            var transitions = this.Where(x => x.Action == ActionTableAction.Transition);
            if (transitions.Any())
            {
                sb.AppendLine("Shift:");
                foreach (var action in transitions.ToArray())
                {
                    sb.AppendLine("\t" + action.ToShortForm());
                }
            }
            var reductions = this.Where(x => x.Action == ActionTableAction.Reduce);
            if (reductions.Any())
            {
                sb.AppendLine("Reduce:");
                foreach (var action in reductions.ToArray())
                {
                    sb.AppendLine("\t" + action.ToShortForm());
                }
            }
            return sb.ToString();
        }

        #region IEnumerable implementation

        public IEnumerator<ActionTableEntry> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            return ToCompressedFormString();
        }
    }

    public abstract class ActionTableEntry
    {

        public ActionTableAction Action { get; protected set; }

        public virtual string ToShortForm()
        {
            throw new NotImplementedException();
        }

        public class ActionTableTransitionEntry : ActionTableEntry
        {

            public ActionTableTransitionEntry(Expression from, ParserState to)
            {
                this.Action = ActionTableAction.Transition;

                From = from;
                To = to;
            }

            public Expression From { get; private set; }

            public ParserState To { get; private set; }

            public override string ToShortForm()
            {
                return string.Format("{0} -> {1}", From, To.Name);
            }

            public override string ToString()
            {
                return string.Format("{0} -> {1}", From, To);
            }
        }

        public class ActionTableReduceEntry : ActionTableEntry
        {

            public ActionTableReduceEntry(string to, Expression from)
            {
                this.Action = ActionTableAction.Reduce;

                To = to;
                From = from;
            }

            public string To { get; private set; }

            public Expression From { get; private set; }

            public override string ToShortForm()
            {
                return string.Format("{0} <- {1}", To, From);
            }

            public override string ToString()
            {
                return string.Format("{0} <- {1}", To, From);
            }
        }
    }

    public enum ActionTableAction
    {
        Transition,
        Reduce
    }
}
