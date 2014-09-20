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
using Sundstrom.Parsing;

namespace Sundstrom
{
	class Program
	{
		public static void Main(string[] args)
		{
			try {
				var grammar = Grammar.ReadFrom(
					File.OpenRead("Grammars/expression2.g"), new ParserOptions() { Root = "root" });
				
				PrintTerminalsAndNonTerminals(grammar);

                Simplify(grammar);

                GenerateParserGenerator(grammar);

				//ParseStates(grammar);
				
				//Parse(grammar);				
				
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

        static void Simplify(Grammar grammar)
        {
            var newGrammar = grammar.ToBnf();
        }

        static void GenerateParserGenerator(Grammar grammar)
        {
            Console.WriteLine(":: PARSER GENERATOR ::");
            Console.WriteLine();

            try
            {
                var generator = new ParserGenerator(grammar);
                var root = generator.Generate();

                Console.WriteLine();
                Console.WriteLine();

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        static void ParseStates(Grammar grammar)
		{
			Console.WriteLine(":: GRAMMAR STATE PARSER ::");
			Console.WriteLine();
			
			try {
				var  parser = new GrammarStateParser(grammar);
				var rootState = parser.ParseStates();
				
				foreach(var state in parser.States) {
					Console.WriteLine("{0}\n", state.ToListFormString());
				}
				
//				Console.WriteLine(rootState);
				
				Console.WriteLine();
				Console.WriteLine();
				
			} catch(Exception exception) {
				Console.WriteLine (exception);
			}
		}
		
		static void Parse(Grammar grammar)
		{
			Console.WriteLine(":: LALR PARSER ::");
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

            foreach (var terminal in grammar.Terminals) {
				Console.WriteLine(terminal.GetValueAsString());
			}
			Console.WriteLine();
			
			Console.WriteLine(":: NON-TERMINALS ::");
			Console.WriteLine();

            foreach (var nonTerminal in grammar.NonTerminals) {
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