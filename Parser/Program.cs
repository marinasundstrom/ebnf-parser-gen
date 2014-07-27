/*
 * Created by SharpDevelop.
 * User: Robert
 * Date: 2014-07-24
 * Time: 16:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using Sundstrom.Ebnf;
using Sundstrom.SyntaxAnalysis;

namespace Sundstrom
{
	class Program
	{
		public static void Main(string[] args)
		{
			try {
				var grammar = Grammar.ReadFrom(
					File.OpenRead("Grammars/test.g"), ParserOptions.AllowForwardReferences);
				
				PrintTerminalsAndNonTerminals(grammar);
				
				Parse(grammar);				
				
			} catch(EbnfParserException e) {
				
				Console.WriteLine(":: EbnfParser Errors ::");
				Console.WriteLine();
				
				foreach(var error in e.Errors) {
					Console.WriteLine("{0}", error);
				}
			}
			
			// TODO: Implement Functionality Here
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}

		static void Parse(Grammar grammar)
		{
			Console.WriteLine(":: PARSER ::");
			Console.WriteLine();

			try {
				var parser = new LRParser(grammar);
				var root = parser.Parse("foo 1");
			} catch(LRParserException exception) {
				Console.WriteLine (exception);
			}
		}
		static void PrintTerminalsAndNonTerminals(Grammar grammar)
		{
			Console.WriteLine(":: TERMINALS ::");
			Console.WriteLine();
			
			foreach (var terminal in grammar.GetTerminals()) {
				Console.WriteLine(terminal.GetValueAsString());
			}
			Console.WriteLine();
			
			Console.WriteLine(":: NON-TERMINALS ::");
			Console.WriteLine();
			
			foreach (var nonTerminal in grammar.GetNonTerminals()) {
				Console.WriteLine(nonTerminal.GetDefinitionAsString());
				Console.WriteLine();
			}
			
			// OutputGrammarToFile (grammar);
			
			Console.WriteLine();
		}

		static void OutputGrammarToFile (Grammar grammar)
		{
			using (Stream stream = File.OpenWrite ("test.out.g")) {
				grammar.WriteTo (stream);
			}
		}
	}
}