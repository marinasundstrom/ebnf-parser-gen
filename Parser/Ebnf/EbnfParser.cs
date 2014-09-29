/*
 * Created by SharpDevelop.
 * User: Robert
 * Date: 2014-07-24
 * Time: 16:01
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using Sundstrom.Parsing;

namespace Sundstrom.Ebnf
{
	/// <summary>
	/// Description of EbnfParser.
	/// </summary>
	public class EbnfParser {
		private Lexer lexer;
		private Grammar grammar;
		private List<Error> errors;
				
		public EbnfParser(){
			Options = new ParserOptions();
		}
		
		List<Terminal> Terminals {
			get;
			set;
		}

		List<NonTerminal> NonTerminals {
			get;
			set;
		}
		
		public IEnumerable<Error> Errors { 
			get {
				return errors;
			}
		}
		
		public ParserOptions Options {
			get; 
			set;
		}
		
		public Grammar Parse(string text) {
			return ReadFrom(
				new MemoryStream(
					Encoding.UTF8.GetBytes(text)));
		}
		
		public Grammar ReadFrom(Stream stream) {
			
			errors = new List<Error>();
			Terminals = new List<Terminal>();
			NonTerminals = new List<NonTerminal>();
			
			using(var streamReader = new StreamReader(stream)) {
				lexer = new Lexer(streamReader);
				
				grammar = new Grammar();
				
				while(!IsEndOfStream) {
					var token = PeekToken();
					
					switch(token.Kind) {
						case TokenKind.Identifier:
							ParseRuleDefinition();
							break;
							
						default:
							ReportError("Expected identifier.", token.GetSourceLocation());
							break;
					}
				}
				
				foreach(var nonTerminal in NonTerminals) {
					if(nonTerminal.Rule == null) {
						ReportError(string.Format("Non-terminal \"{0}\" has not been declared.", nonTerminal.Name));
					}
				}
				
				if(errors.Any()) {
					if(Options.ThrowExceptionOnErrors) {
						throw new EbnfParserException(errors);
					} else {
						grammar = null;;
					}
				}
				
				return grammar;
			}
		}

		Error ReportError(string message, SourceLocation sourceLocation)
		{
			var error = new Error(message, sourceLocation);
			errors.Add(error);
			return error;
		}
		
		Error ReportError(string message)
		{
			var error = new Error(message);
			errors.Add(error);
			return error;
		}
		
		bool Expect(string str, out Error error)
		{
			Token token;
			return Expect(str, out token, out error);
		}			
		
		bool Expect(string str)
		{
			Token token;
			return Expect(str, out token);
		}
				
		bool Expect(string str, out Token token)
		{
			Error error = null;
			return Expect(str, out token, out error);
		}

		bool Expect(string str, out Token token, out Error error)
		{
			token = PeekToken();
			if(token.Value == str) {
				ReadToken();
				error = null;
				return true;
			}
			error = ReportError(
				string.Format("Expected \"{0}\".", str), 
				token.GetSourceLocation());
			return false;
		}
		
		void ParseRuleDefinition()
		{
			var identifierToken = ReadToken();
			if(identifierToken.Kind != TokenKind.Identifier) {
				ReportError("Expected identifier.", 
				            identifierToken.GetSourceLocation());
			}
			Token assignToken;
			if(Expect("=", out assignToken)) {
				NonTerminal ruleDef = null;
				var identifier = identifierToken.Value;
				if(!(identifier == Options.Root || identifier == "Grammar" || identifier == "grammar")) {
					ruleDef = NonTerminals.FirstOrDefault(nt => nt.Name == identifier);
					if(ruleDef != null) {
						if(ruleDef.Rule != null) {
							ReportError(string.Format("Non-terminal \"{0}\" has already been declared.", identifier), identifierToken.GetSourceLocation());
						}
					} else {
						ruleDef = new NonTerminal(identifier);
						NonTerminals.Add(ruleDef);
					}
				}
				
				var rule = ParseRule();
				Token semicolonToken;
				if(Expect(";", out semicolonToken)) {
					if(identifier == Options.Root || identifier == "Grammar" || identifier == "grammar") {
						if(grammar.Root != null) {
							ReportError("Grammar has already been assigned to.", assignToken.GetSourceLocation());
						} else {
							grammar.Root = new  NonTerminal(Options.Root) { 
								Rule =  rule 
							};
						}
					} else {
						ruleDef.Rule = rule;
					}
				}
			}
		}

		bool IsOperator(Token token)
		{
			switch(token.Kind) {
				case TokenKind.Comma:
				//case TokenKind.Option:
					return true;
			}
			return false;
		}

		int GetPrecedence(TokenKind kind)
		{
			switch(kind) {
				case TokenKind.Comma:
                    //return 1;
                    return 0;

                    //case TokenKind.Option:
                    //	return 0;
            }
			return -1;
		}

        Expression ParseRule()
        {
            Expression ret = ParseRule2();
            while (true)
            {
                var token = PeekToken();
                switch (token.Kind)
                {
                    case TokenKind.Option:
                        ReadToken();
                        break;

                    default:
                        return ret;
                }
                var rhs = ParseRule();
                ret = new Alternation(ret, rhs);
            }
        }


        Expression ParseRule2()
        {
            Expression ret = ParseRuleExpression();
            while (true)
            {
                var token = PeekToken();
                switch (token.Kind)
                {
                    case TokenKind.Comma:
                        ReadToken();
                        break;

                    default:
                        return ret;
                }
                var rhs = ParseRule2();
                ret = new Concatenation(ret, rhs);
            }
        }


        Expression ParseRuleExpression(int prec = 0)
		{
            //var left = ParsePrimary();
            //while (true) {
            //	var token = PeekToken ();
            //	if (IsOperator (token)) {
            //		var precedence = GetPrecedence (token.Kind);
            //		if (precedence >= prec) {
            //			ReadToken ();
            //			var right = ParseRuleExpression(prec + 1);
            //			switch (token.Kind) {
            //				case TokenKind.Comma:
            //					left = new Concatenation (left, right);
            //					break;
            //			}
            //		} else {
            //			return left;
            //		}
            //	} else {
            //		return left;
            //	}
            //}
            return ParsePrimary();

        }

        Expression ParseGrouping()
		{
			var rule = ParseRule();
			Error error;
			if(!Expect(")", out error)) {
				return new ErrorNode(error);
			}
			return new Grouping(rule);;
		}

		void Error_ForwardReference()
		{
			ReportError("Forward references are not allowed. Make sure the feature has been enabled.");
		}

		Expression ParsePrimary()
		{
			var token = ReadToken();
			switch(token.Kind){
				case TokenKind.Identifier:
					var nonTerminal = NonTerminals.FirstOrDefault(nt => nt.Name == token.Value);
					if(nonTerminal == null) {
						if(Options.AllowForwardReferences) {
							nonTerminal = new NonTerminal(token.Value);
							NonTerminals.Add(nonTerminal);
						} else {
							var error = ReportError(string.Format("Non-terminal \"{0}\" has not been declared.", token.Value), 
							                        token.GetSourceLocation());
							
							Error_ForwardReference();
							
							return new ErrorNode(error);
						}
					}
					return nonTerminal;
					
				case TokenKind.LeftBrace:
					return ParseRepetition();
					
				case TokenKind.LeftParens:
					return ParseGrouping();
					
				case TokenKind.DoubleQuote:
					return ParseTerminal('\"');
					
				case TokenKind.SingleQuote:
					return ParseTerminal('\'');
					
				default:
					var error2 = ReportError("Unexpected token.", token.GetSourceLocation());
					return new ErrorNode(error2);
					
			}
		}

		Expression ParseRepetition()
		{
			var rule = ParseRule();
			Error error;
			if(!Expect("}", out error)) {
				return new ErrorNode(error);
			}
			return new Repetition(rule);;
		}
		Expression ParseTerminal(char c)
		{
			StringBuilder sb = new StringBuilder();
			Token token = PeekToken();
			while(token.Value != c.ToString() && !IsEndOfStream) {
				ReadToken();
				sb.Append(token.Value);
				token = PeekToken();
			}
			Error error;
			if(!Expect(c.ToString(), out error)) {
				return new ErrorNode(error);
			}
			var str = sb.ToString();
			var terminal = Terminals.FirstOrDefault(t => t.Value == str);
			if(terminal == null) {
				terminal = new Terminal(str);
				Terminals.Add(terminal);
			}
			return terminal;
		}
		
		private Token  PeekToken() {
			return lexer.Peek();
		}
		
		private Token  ReadToken() {
			return lexer.Read();
		}
		
		private bool IsEndOfStream {
			get {
				return PeekToken().Kind == TokenKind.EndOfStream;
			}
		}
		
		class Lexer {
			StreamReader StreamReader { get; set; }
			Token? Lookahead { get; set; }
			
			public Lexer(StreamReader streamReader) {
				StreamReader = streamReader;
				
				Line = 1;
				Column = 1;
			}
			
			public Token Read() {
				if(Lookahead != null) {
					var temp = Lookahead;
					Lookahead = null;
					return temp.GetValueOrDefault();;
				}
				return ReadCore();
			}
			
			public Token Peek() {
				if(Lookahead == null) {
					Lookahead = ReadCore();
				}
				return Lookahead.GetValueOrDefault();
			}
			
			int Line { get; set; }
			int Column { get; set; }
			
			private Token ReadCore() {
				while(!IsEoS) {
					int ln = Line;
					int col = Column;
					
					var ch = PeekChar();
					if(char.IsLetter(ch)) {
						var sb = new StringBuilder();
						while(char.IsLetterOrDigit(ch) || ch == '_') {
							ReadChar();
							sb.Append(ch);
							ch = PeekChar();
						}
						return new Token(TokenKind.Identifier, sb.ToString(), ln, col);
					} else if(char.IsDigit(ch)) {
						var sb = new StringBuilder();
						while(char.IsDigit(ch)) {
							ReadChar();
							sb.Append(ch);
							ch = PeekChar();
						}
						return new Token(TokenKind.Number, sb.ToString(), ln, col);
					} else {
						ReadChar();
						switch(ch) {
							case '[':
								return new Token(TokenKind.LeftSquare, "[", ln, col);
							case ']':
								return new Token(TokenKind.RightSquare, "[", ln, col);
							case '{':
								return new Token(TokenKind.LeftBrace, "{", ln, col);
							case '}':
								return new Token(TokenKind.RightBrace, "}", ln, col);
							case '(':
								return new Token(TokenKind.LeftParens, "(", ln, col);
							case ')':
								return new Token(TokenKind.RightParens, ")", ln, col);
							case '<':
								return new Token(TokenKind.LeftAngle, "<", ln, col);
							case '>':
								return new Token(TokenKind.RightAngle, ">", ln, col);
							case '\'':
								return new Token(TokenKind.SingleQuote, "\'", ln, col);
							case '\"':
								return new Token(TokenKind.DoubleQuote, "\"", ln, col);
							case '=':
								return new Token(TokenKind.Assign, "=", ln, col);
							case '|':
								return new Token(TokenKind.Option, "|", ln, col);
							case '.':
								return new Token(TokenKind.Dot, ".", ln, col);
							case ',':
								return new Token(TokenKind.Comma, ",", ln, col);
							case ';':
								return new Token(TokenKind.Semicolon, ";", ln, col);
							
							case  '\t':
								break;								
							case  '\r':
								break;
							case '\n':
								break;
								
							case ' ':
								break;
								
							default:
								return new Token(TokenKind.Misc, ch.ToString(), ln, col);
						}
					}
				}
				return new Token(TokenKind.EndOfStream, Line, Column);
			}
			
			private char PeekChar() {
				return (char)StreamReader.Peek();
			}
			
			private char ReadChar() {
				var ch = (char)StreamReader.Read();
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
			
			private bool IsEoS {
				get {
					return StreamReader.Peek() == -1;
				}
			}
		}
		
		internal struct Token {
			private readonly string value;
			private TokenKind kind;
			private int ln;
			private int col;
			
			public Token(TokenKind kind, int ln, int col) {
				this.kind = kind;
				this.value = null;
				this.ln = ln;
				this.col = col;
			}
			
			public Token(TokenKind kind, string value, int ln, int col) {
				this.kind = kind;
				this.value = value;
				this.ln = ln;
				this.col = col;
			}

			public TokenKind Kind {
				get {
					return kind;
				}
			}
			
			public string Value {
				get {
					return value;
				}
			}
			
			public int Line {
				get {
					return ln;
				}
			}
			
			public int Column {
				get {
					return col;
				}
			}
			
			public override string ToString()
			{
				if(value == null){
					return string.Format("{0}", kind);	
				}
				return string.Format("{0}", value);
			}
		}
		
		public enum TokenKind {
			EndOfStream,
			NewLine,
			Identifier,
			Number,
			LeftSquare,
			RightSquare,
			LeftBrace,
			RightBrace,
			LeftParens,
			RightParens,
			LeftAngle,
			RightAngle,
			SingleQuote,
			DoubleQuote,
			Assign,
			Option,
			Dot,
			Comma,
			Semicolon,
			Misc
		}
	}
	
	public sealed class ParserOptions {
		public ParserOptions() {
			AllowForwardReferences = true;
			AutoIdentifyRoot = false;
			Root = "grammar";
			ThrowExceptionOnErrors = true;
		}
		
		public bool AllowForwardReferences { get; set; }
		public bool AutoIdentifyRoot { get; set; }
		public string Root { get; set; }
		public bool ThrowExceptionOnErrors { get; set; }
	}
	
	public class GrammarInfoAttribute : Attribute {
		public GrammarInfoAttribute(string name, string author, string description) {
			Name = name;
			Author = author;
			Description = description;
		}
		
		public string Name { get; private set; }
		
		public string Author { get; private set; }
		
		public string Description { get; private set; }
	}
	
	public class Grammar
	{
		NonTerminal root = null;
		NonTerminal realRoot = null;
		
		List<Terminal> terminals = new List<Terminal>();
		List<NonTerminal> nonTerminals = new List<NonTerminal>();
		
		static Grammar() {
            EOS = new Terminal("EOS", "\0");
            EOL = new Terminal("EOL", "\n");
			SyntaxError =  new Terminal("SYNTAX_ERROR");
		}
		
		public string Name {
			get {
				return GetGrammarInfoAttributeValue("Name");
			}
		}
		
		public string Author {
			get {
				return GetGrammarInfoAttributeValue("Author");
			}
		}
		
		public string Description {
			get {
				return GetGrammarInfoAttributeValue("Description");
			}
		}

		string GetGrammarInfoAttributeValue(string name)
		{
			var grammarInfoAttributeType = typeof(GrammarInfoAttribute);
			var attr = this.GetType()
				.GetCustomAttributes(grammarInfoAttributeType, false)
				.FirstOrDefault();
			if(attr != null){
				return (string)grammarInfoAttributeType.GetProperty(name).GetValue(attr);
			}
			
			return null;
		}
		
		internal string RootName {
			get {
				return root.Name;
			}
			set {
				root.Name = value;
				UpdateRealRootName();
			}
		}
		
		/// <summary>
		/// Gets the root of this grammar as assigned in the object.
		/// </summary>
		public NonTerminal Root 
		{
			get {
				return root;
			}
			
			set {
				if(root != null) {
					throw new InvalidOperationException("Root has already been assigned to.");
				}
				if(value == null)  {
					throw new ArgumentNullException();
				}
				root = value;
			}
		}

		void UpdateRealRootName()
		{
			if (!realRoot.Name.StartsWith(root.Name)) {
				realRoot.Name = string.Format("{0}'", root.Name);
			}
		}
		
		/// <summary>
		/// Gets the real root of this grammar. (Returns Root if no changes have been applied)
		/// </summary>
		public NonTerminal RealRoot {
			get {
				if(realRoot == null) {
                    var concatenation = (Concatenation)root.Rule;
                    var lastNodeInExpression = concatenation.Concatenations().Last();
					if(lastNodeInExpression.GetValueAsString() == Grammar.EOL.GetValueAsString()) {
						realRoot = root;
					} else {
						realRoot = new NonTerminal(
							string.Format("{0}'", root.Name));
						realRoot.Rule = root + Grammar.EOS;
					}
				} 
				return realRoot;
			} 
		}

        public static Terminal EOS { get; protected set; }

        public static Terminal EOL { get; protected set; }
		
		public static Terminal SyntaxError { get; protected set; }
		
		/// <summary>
		/// Parses grammar from string.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static Grammar Parse(string text)
		{
			return Parse(text, new ParserOptions());
		}
		
		/// <summary>
		/// Parses grammar from string.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static Grammar Parse(string text, ParserOptions options)
		{
			return ReadFrom(
				new MemoryStream(
					Encoding.UTF8.GetBytes(text)), options);
		}
		
		/// <summary>
		/// Reads grammar from stream as text.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Grammar ReadFrom(Stream stream)
		{
			return ReadFrom(stream, new ParserOptions());
		}

		/// <summary>
		/// Reads grammar from stream as text.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static Grammar ReadFrom(Stream stream, ParserOptions options)
		{
			var parser = new EbnfParser();
			parser.Options = options;
			return parser.ReadFrom(stream);
		}
		
		/// <summary>
		/// Writes grammar to stream as text.
		/// </summary>
		/// <param name="stream"></param>
		public void WriteTo(Stream stream) {
//			RootName = "root";
			stream.SetLength(0);
			StreamWriter writer = new StreamWriter(stream);
			writer.AutoFlush = true;
            foreach (var nonTerminal in NonTerminals) {
				writer.WriteLine(nonTerminal.GetDefinitionAsString());
				writer.WriteLine();
			}
		}

		/// <summary>
		/// Gets all terminals.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Terminal> Terminals
        { 
            get
            {
                TryProcessNode();
                return terminals;
            }
		}
		
		/// <summary>
		/// Gets all non-terminals.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<NonTerminal> NonTerminals
        { 
            get
            {
                TryProcessNode();
                return nonTerminals;
            }
		}
		
		/// <summary>
		/// Treat the specified text as a terminal.
		/// </summary>
		/// <param name="terminal"></param>
		/// <returns></returns>
		protected static Terminal Term(string terminal) {
			return Rule.Terminal(terminal);
		}
		
		protected static Repetition Repeat(Expression rule){
			return rule.Repeat();
		}
		
		protected static Option Option(Expression rule){
			return rule.Option();
		}
		
		protected static Grouping Group(Expression rule){
			return Rule.Group(rule);
		}

		void TryProcessNode()
		{
			if (!nonTerminals.Any() || !terminals.Any()) {
				if(Root == null) {
					throw new NullReferenceException("Root cannot be null.");
				}
				ProcessNode(RealRoot);
			}
		}
		void ProcessNode (Expression node) {
			var terminal = node as Terminal;
			if (terminal != null) {
				if(!terminals.Contains(terminal)){
					terminals.Add(terminal);
				}				
			} else {
				var nonTerminal = node as NonTerminal;
				if (nonTerminal != null) {
					if(!nonTerminals.Contains(nonTerminal)){
						nonTerminals.Add(nonTerminal);
						ProcessNode (nonTerminal.Rule);
					}
				} else {
					var concatenation = node as Concatenation;
					if (concatenation != null) {
						ProcessNode (concatenation.Left);
						ProcessNode (concatenation.Right);
					} else {
						var alternation = node as Alternation;
						if (alternation != null) {
							ProcessNode (alternation.Left);
							ProcessNode (alternation.Right);
						} else {
							var option = node as Option;
							if (option != null) {
								ProcessNode (option.Node);
							} else {
								var repetition = node as Repetition;
								if (repetition != null) {
									ProcessNode (repetition.Node);
								} else {
									var grouping = node as Grouping;
									if (grouping != null) {
										ProcessNode (grouping.Node);
									}
								}
							}
						}
					}
				}
			}
		}
		
		public void Analyze() {
			throw new NotImplementedException();
		}
	}

	public abstract class Expression
	{
		public static Concatenation operator + (Expression left, Expression right)
		{
			return new Concatenation (left, right).RightFactor();
		}

		public static Alternation operator | (Expression left, Expression right)
		{
            return new Alternation (left, right).RightFactor();
		}

		public static implicit operator Expression (string value)
		{
			return new Terminal (value);
		}

        public virtual Expression RightFactor()
        {
            return this;
        }
		
		public virtual string GetValueAsString () {
            //throw new NotImplementedException();

            return string.Empty;
		}
	}

	public class NonTerminal : Expression
	{
		public NonTerminal (string name)
		{
			Name = name;
		}

		public string Name { get; internal set; }

		Expression rule = null;
		public Expression Rule {
			get {
				return rule;
			}
			set {
				if(rule != null) {
					throw new InvalidOperationException("Expression has already been set.");
				}
				rule = value;
			}
		}
		
		public string GetDefinitionAsString() {
			return string.Format("{0} = {1} ;", Name, Rule);
		}
		
		public override string GetValueAsString()
		{
			return Name;
		}

		public override string ToString ()
		{
			return string.Format("{0}", Name);
		}
	}

	public class Terminal : Expression
	{
		public Terminal (string value)
		{
			Value = value;
		}
		
		public Terminal (string name, string value)
			: this(value)
		{
			Name = name;
		}
			
		public string Name { get; private set; }

		public string Value { get; private set; }

		public static implicit operator Terminal (string value)
		{
			return new Terminal (value);
		}
		
		public override string GetValueAsString()
		{
			return Value;
		}

		public override string ToString ()
		{
			if(Name != null) {
				return Name;
			}
			
			return string.Format("\"{0}\"", Value);
		}
	}
	
	public class ErrorNode : Expression {
		public ErrorNode(Error error) {
			Error = error;
		}
		
		public Error Error { get; private set; }
		
		public override string ToString()
		{
			return string.Format("ERROR: {0}", Error);
		}
	}

	public static class Rule
	{
		public static Grouping Group (Expression node)
		{
			return new Grouping (node);
		}

		public static Repetition Repeat (Expression node)
		{
			return new Repetition (node);
		}

		public static Terminal Terminal (string value)
		{
			return new Terminal (value);
		}

		public static NonTerminal NonTerminal(string name, Expression rule) {
			var nonTerminal = new NonTerminal (name);
			nonTerminal.Rule = rule;
			return nonTerminal;
		}
	}

	public sealed class Concatenation : Expression
	{
		public Concatenation (Expression left, Expression right)
		{
			Left = left;
			Right = right;
		}

		public Expression Left { get; private set; }

		public Expression Right { get; private set; }
	
		public override string GetValueAsString () {
			return string.Empty;
		}

        public override Expression RightFactor()
        {
            //var alternation = this as Alternation;
            //if (alternation != null)
            //{

            //}
            //else
            //{
            //    var concatenation = this as Concatenation;
            //    if (concatenation != null)
            //    {

            //    }
            //}

            return base.RightFactor();
        }

        public override string ToString()
		{
			return string.Format("{0}, {1}", Left, Right);
		}
	}

	public sealed class Alternation : Expression
	{
		public Alternation (Expression left, Expression right)
		{
			Left = left;
			Right = right;
		}

		public Expression Left { get; private set; }

		public Expression Right { get; private set; }
		
		public override string GetValueAsString () {
			return string.Empty;
		}

        public override Expression RightFactor()
        {
            if (Left is Alternation)
            {

            }

            return base.RightFactor();
        }

        public override string ToString()
		{
			return string.Format("{0} | {1}", Left, Right);
		}
	}

	public sealed class Option : Expression
	{
		public Option (Expression node)
		{
			Node = node;
		}

		public Expression Node { get; private set; }
		
		public override string ToString()
		{
			return string.Format("[{0}]", Node);
		}
	}

	public sealed class Repetition : Expression
	{
		public Repetition (Expression node)
		{
			Node = node;
		}

		public Expression Node { get; private set; }
		
		public override string ToString()
		{
			return string.Format("{{{0}}}", Node);
		}
	}

	public sealed class Grouping : Expression
	{
		public Grouping (Expression node)
		{
			Node = node;
		}

		public Expression Node { get; private set; }
		
		public override string ToString()
		{
			return string.Format("({0})", Node);
		}
	}

	
	public struct SourceLocation {
		int _line;
		int _column;
		
		public SourceLocation(int line, int column) {
			_line = line;
			_column = column;
		}
		
		public int Line { 
			get {
				return _line;
			}
		}
		
		public int Column { 
			get {
				return _column;
			}
		}
	}
	
	public class Error {
		internal Error(string message, SourceLocation sourceLocation) {
			Message = message;
			SourceLocation = sourceLocation;
		}
		
		internal Error(string message) {
			Message = message;
		}
		
		public string Message { get; private set; }
		
		public SourceLocation SourceLocation { get; private set; }
		
		public override string ToString()
		{
			if(SourceLocation.Equals(default(SourceLocation))) {
				return string.Format("{0}", Message);
			}
			return string.Format("{0} ({1}, {2})", Message, SourceLocation.Line, SourceLocation.Column);
		}
	}
	
	static class TokenExtensions {
		public static SourceLocation GetSourceLocation(this EbnfParser.Token token) {
			return new SourceLocation(token.Line, token.Column);
		}
	}
	
	public sealed class EbnfParserException : Exception {
		public EbnfParserException(IEnumerable<Error> errors)
			: base ("Malformed EBNF was encountered.") {
			Errors = errors;
		}
		
		public IEnumerable<Error> Errors { 
			get; 
			private set; 
		}
	}
}
