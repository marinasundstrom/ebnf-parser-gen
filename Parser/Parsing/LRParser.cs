/*
 * Created by SharpDevelop.
 * User: Robert
 * Date: 2014-07-25
 * Time: 11:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sundstrom.Ebnf;

namespace Sundstrom.Parsing
{
	/// <summary>
	/// Description of LRParser.
	/// </summary>
	public sealed class LRParser
	{	
			
		public LRParser(Grammar grammar)
		{
			Grammar = grammar;
			StateParser = new GrammarStateParser(Grammar);
			
			GenerateStates();
		}
		
		private ParserState FirstState { get; set; }
		
		private GrammarStateParser StateParser { get; set; }

		void GenerateStates() {
			FirstState = StateParser.ParseStates();
		}

		
		public Grammar Grammar { get; private set; }
		
		public IEnumerable<ParserState> States { 
			get {
				return States;
			}
		}
			
		public ParseNode Parse(string text) {
			return ReadFrom(
				new MemoryStream(
					Encoding.UTF8.GetBytes(text)));
		}
		
		public ParseNode ReadFrom(Stream stream) {
			Line = 1;
			Column = 1;
			
			StreamReader = new StreamReader(stream);		
			StateStack = new Stack<ParseNode>();
			
			return null;
		}
		
		bool IsEndOfStream { 
			get {
				return StreamReader.EndOfStream;
			}
		}
		
		StreamReader StreamReader { get; set; }
		
		Stack<ParseNode> StateStack { get; set; }
		
		private void Shift () {
			if(IsEndOfStream) {
				StateStack.Push(
					new TerminalNode("EOF"));
			}
				
			var ch = ReadChar();
			StateStack.Push(
				new TerminalNode(ch.ToString()));
		}	
		
		private void Reduce() {
		
		}
		
		private void Error () {
			
		}
		
		int Line { get; set; }
		int Column { get; set; }
		
		private char Lookahead() {
			return (char)StreamReader.Peek();
		}
			
		private char ReadChar() {
			int i = StreamReader.Read();
			var ch = (char)i;
			if(ch == '\n') {
				Line ++;
				Column = 1;
			} if(ch == '\t') { 
				Column += 4;
			} else {
				Column ++;
			}
			return ch;
		}		
	}
	
	public class ParserState {
		public ParserState(string name) {
			Name = name;		
			ActionTable = new ActionTable();
		}
			
		public string Name { get; private set; }
			
		public ActionTable ActionTable { get; private set; }
			
		public override string ToString()
		{
			if(!ActionTable.Any())
				return Name;
			
			return string.Format("({0}: {1})", Name, ActionTable);
		}
	}
	
	public class ActionTable : IEnumerable<ActionTableEntry> {
		
		private List<ActionTableEntry> items = new List<ActionTableEntry>();
		
		public ActionTable() {
			
		}
	
		internal void AddTransition(Expression from, ParserState to) {
			items.Add(new ActionTableEntry.ActionTableTransitionEntry(from, to));
		}
		
		internal void AddReduce(string to, Expression from) {
			items.Add(new ActionTableEntry.ActionTableReduceEntry(to, from));
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
			var sb = new StringBuilder();
			var transitions = this.Where(x => x.Action == ActionTableAction.Transition);
			foreach(var action in transitions.ToArray()) {
				sb.Append(action.ToString());
				if(action != transitions.Last()) {
					sb.Append(",");
				}
			}
			return sb.ToString();
		}
	}
	
	public abstract class ActionTableEntry {
		
		public ActionTableAction Action { get; protected set; }
			
		public class ActionTableTransitionEntry : ActionTableEntry {
			
			public ActionTableTransitionEntry(Expression from, ParserState to) {
				this.Action = ActionTableAction.Transition;
				
				From = from;
				To = to;
			}
			
			public Expression From { get; private set; }
			
			public ParserState To { get; private set; }
			
			public override string ToString()
			{
				return string.Format("{0} -> {1}", From.GetValueAsString(), To);
			}
		}
	
		public class ActionTableReduceEntry : ActionTableEntry {
			
			public ActionTableReduceEntry(string to, Expression from) {
				this.Action = ActionTableAction.Reduce;
				
				To = to;
				From = from;
			}
			
			public string To { get; private set; }
			
			public Expression From { get; private set; }
			
			public override string ToString()
			{
				return string.Format("{0} <- {1}", To, From.GetValueAsString());
			}
		}
	}
	
	public enum ActionTableAction {
		Transition,
		Reduce
	}
	
	public static class SequenceExtensions {
		public static IEnumerable<Expression> EnumerateConcatenated(this Expression node) {
			var concatenation = node as Concatenation;
			if(concatenation != null) {
				yield return concatenation.Left;
				var result = EnumerateConcatenated(
					concatenation.Right);
				foreach(var r in result) {
					yield return r;
				}
			} else {
				yield return node;
			}
		}
		
		public static IEnumerable<Expression> EnumerateAlternations(this Expression node) {
			var alternation = node as Alternation;
			if(alternation != null) {
				yield return alternation.Left;
				var result = EnumerateAlternations(
					alternation.Right);
				foreach(var r in result) {
					yield return r;
				}
			} else {
				yield return node;
			}
		}
	}

	public class LRParserException : Exception {

	}

	public enum NodeKind {
		Terminal,
		NonTerminal
	}
	
	public abstract class ParseNode {
		public abstract NodeKind NodeKind { get; }		
		public abstract string StringRepresentation { get; }
		public abstract string Name { get; }
	}
	
	public sealed class TerminalNode : ParseNode {
		public TerminalNode(string value) {
			Value = value;
		}
		
		public string Value { get; private set; }

		public override NodeKind NodeKind {
			get {
				return NodeKind.Terminal;
			}
		}
		
		public override string StringRepresentation {
			get {
				return Value;
			}
		}
		
		public override string Name {
			get {
				return Value;
			}
		}
		
		public override string ToString()
		{
			return string.Format("{0} (Terminal)", Value);
		}
	}
	
	public sealed  class NonTerminalNode : ParseNode {
		public NonTerminalNode(NonTerminal nonTerminal, IEnumerable<ParseNode> childNodes) {
			NonTerminal = nonTerminal;
			ChildNodes = childNodes;
		}
		
		public NonTerminal NonTerminal { get; private set; }
		
		public IEnumerable<ParseNode> ChildNodes { get; private set; }
		
		public override NodeKind NodeKind {
			get {
				return NodeKind.NonTerminal;
			}
		}
		
		public override string Name {
			get {
				return NonTerminal.Name;
			}
		}
		
		public override string StringRepresentation {
			get {
				var sb = new StringBuilder();
				foreach(var node in ChildNodes) {
					sb.AppendFormat("{0}", node.StringRepresentation);
				}
				return sb.ToString();
			}
		}
		
		public override string ToString()
		{
			return string.Format("{0} ({1})", StringRepresentation, NonTerminal.Name);
		}
	}
}
