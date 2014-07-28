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
using Sundstrom.Ebnf;

namespace Sundstrom.Parsing
{
	/// <summary>
	/// Description of GrammarStateParser.
	/// </summary>
	public sealed class GrammarStateParser {
		
		public GrammarStateParser(Grammar grammar) {
			Grammar = grammar;
		}
		
		public Grammar Grammar { get; private set; }
		
		private List<ParserState> States { get; set; }
		
		private Stack<ParserState> StateStack { get; set; }
		
		public ParserState CurrentState {
			get {
				return StateStack.Peek();
			}
		}
		
		public ParserState ParseStates() {				
			return ParseStatesCore();
		}

		ParserState ParseStatesCore()
		{
			States = new List<ParserState>();
			StateStack = new Stack<ParserState>();
			
			// Create root.
			
			var root = Grammar.RealRoot;
			
			var queue = new Queue<Expression>(
				root.Rule.EnumerateConcatenated());
			
			ProcessState(queue);
			
			var state = PopState();
			return state;
		}
		
		ParserState PopState() {
			return StateStack.Pop();
		}
		
		ParserState CreateState() {
			return CreateState(null);
		}
		
		ParserState CreateState(NonTerminal nonTerminal) {
			var state = new ParserState(string.Format("S{0}", States.Count));
			States.Add(state);
			StateStack.Push(state);
			return state;
		}
		
		void ProcessState(Queue<Expression> input)
		{	
			input = new Queue<Expression>(input);
			
			while(input.Count > 0) {
				var lookaheadNode = input.Peek();
				
				ParserState state;
				var terminal = lookaheadNode as Terminal;
				if(terminal != null) {
					input.Dequeue();
					state = CreateState();
					CurrentState.ActionTable.AddTransition(lookaheadNode, state);
				} else {
					var nonTerminal = lookaheadNode as NonTerminal;
					if(nonTerminal != null) {
						var queue2 = new Queue<Expression>(
							nonTerminal.Rule.EnumerateConcatenated());
						state = CreateState();
						ProcessState(queue2);
						input.Dequeue();
					} else {
						var alternation = lookaheadNode as Alternation;
						if(alternation != null) {
							var alternations = alternation.EnumerateAlternations();
							foreach(var node in alternations) {
								var queue3 = new Queue<Expression>(
									node.EnumerateConcatenated());
									
							    ProcessState(queue3);
							}
							input.Dequeue();
						} else {
							throw new NotSupportedException();
						}
					}
				}
			}
			
			if(StateStack.Count > 0) 
				PopState();
		}
	}
}
