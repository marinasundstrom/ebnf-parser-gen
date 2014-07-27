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

namespace Sundstrom.SyntaxAnalysis
{
	/// <summary>
	/// Description of LRParser.
	/// </summary>
	public class LRParser
	{
		public class ParserState {
			public ParserState(string name, NonTerminal nonTerminal) {
				Name = name;
				NonTerminal = nonTerminal;
				
				ItemsToShift = new Dictionary<Node, ParserState>();
				ItemsToReduce = new Dictionary<Node, NonTerminal>();
				
				Transitions = new Dictionary<Node, ParserState>();
			}
			
			public string Name { get; private set; }
			
			public NonTerminal NonTerminal  { get; private set; }
			
			public IDictionary<Node, ParserState> ItemsToShift { get; private set; }

			public IDictionary<Node, NonTerminal> ItemsToReduce { get; private set; }
			
			public IDictionary<Node, ParserState> Transitions { get; private set; }
			
			public override string ToString()
			{
				return string.Format("{0}", Name);
			}

		}
		
		private List<ParserState> states;
		
		public LRParser(Grammar grammar)
		{
			Grammar = grammar;
			
			GenerateStates();
		}

		void GenerateStates() {
			states = new List<ParserState>();
			
			// Create root.
			
			var root = Grammar.RealRoot;
			
			var queue = new Queue<Node>(
				root.Rule.EnumerateConcatenated());
			
			var parserState = CreateState();
			
			GenerateState(parserState, queue);
		}

		void GenerateState(ParserState outerState, Queue<Node> queue)
		{		
			//states.Add(parserState);
			
			var lookaheadNode = queue.Peek();
			
			var terminal = lookaheadNode as Terminal;
			if(terminal != null) {
				queue.Dequeue();
				var state = CreateState();
				outerState.Transitions.Add(lookaheadNode, state);
			} else {
				var nonTerminal = lookaheadNode as NonTerminal;
				if(nonTerminal != null) {
					queue.Dequeue();
					var queue2 = new Queue<Node>(
						nonTerminal.Rule.EnumerateConcatenated());
					var lookaheadNode2 = queue2.Peek();
					GenerateState(outerState, queue2);
				} else {
					var alternation = lookaheadNode as Alternation;
					if(alternation != null) {
						var alternations = alternation.EnumerateAlternations();
						foreach(var node in alternations) {
							var queue3 = new Queue<Node>(
								node.EnumerateConcatenated());
						    GenerateState(outerState, queue3);							
						}
					} else {
						throw new NotSupportedException();
					}
				}
			}
		}
		
		private ParserState CreateState() {
			return CreateState(null);
		}
		
		private ParserState CreateState(NonTerminal nonTerminal) {
			return new ParserState(string.Format("S{0}", states.Count), nonTerminal);
		}
		
		void ProcessStates(List<Rule> alternations, Rule rule) {
			var alternation = rule as Alternation;
			if(alternation != null) {
				ProcessStates(alternations, (Rule)alternation.Left);
				ProcessStates(alternations, (Rule)alternation.Right);
			} else {
				alternations.Add(rule);
			}
		}
		
		public Grammar Grammar { get; private set; }
		
		public IEnumerable<ParserState> States { 
			get {
				return states;
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
			ParseStack = new Stack<ParseNode>();
			
			return null;
		}
		
		bool IsEndOfStream { 
			get {
				return StreamReader.EndOfStream;
			}
		}
		
		StreamReader StreamReader { get; set; }
		
		Stack<ParseNode> ParseStack { get; set; }
		
		private void Shift () {
			if(IsEndOfStream) {
				ParseStack.Push(
					new TerminalNode("EOF"));
			}
				
			var ch = ReadChar();
			ParseStack.Push(
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
	
	public static class SequenceExtensions {
		public static IEnumerable<Node> EnumerateConcatenated(this Node node) {
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
		
		public static IEnumerable<Node> EnumerateAlternations(this Node node) {
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
