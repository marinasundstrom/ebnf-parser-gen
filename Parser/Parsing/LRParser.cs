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
