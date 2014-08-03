/*
 * Created by SharpDevelop.
 * User: Robert
 * Date: 08/01/2014
 * Time: 23:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Sundstrom.Ebnf;

namespace GrammarExplorer.ViewModels
{
	/// <summary>
	/// Description of MainViewModel.
	/// </summary>
	public class MainViewModel : INotifyPropertyChanged
	{
		public MainViewModel()
		{
			
		}
		
		public string GrammarText { get; set; }
		
		public void ParseGrammar() {
			try {
				Grammar = Grammar.Parse(GrammarText);
			} catch (EbnfParserException exc) {
				foreach(var error in exc.Errors) 
					ParserErrors.Add(error);
			}
		}
		
		public Grammar Grammar { get; private set; }
		
		public ObservableCollection<Error> ParserErrors { get; private set; }

		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
