private ParserState ParseNonTerminal(NonTerminal nonTerminal)
{
    var queue = nonTerminal.Rule.EnumerateConcatenated().Skip(1).ToQueue();

    var states = new List<ParserState>();

    var firstState = CreateState();

    states.Add(firstState);

    ParserState newState = null;
    ParserState lastState = firstState;

    while (queue.Any())
    {
        var term = queue.Dequeue();

        var terminal = term as Terminal;
        if (terminal != null)
        {
            newState = CreateState();
            if (lastState != null)
            {
                lastState.ActionTable.AddTransition(term, newState);
            }
            states.Add(newState);
        }
        else
        {
            var nt = term as NonTerminal;
            if (nt != null)
            {
                newState = CreateState();
                if (lastState != null)
                {
                    lastState.ActionTable.AddTransition(term, newState);
                }
                lastState.SetNonTerminal(nt);
                states.Add(newState);
            }
            else
            {
                var alternation = term as Alternation;
                if (alternation != null)
                {
                    foreach (var @case in alternation.EnumerateAlternations())
                    {

                    }
                }
                else
                {
                    //throw new NotImplementedException();
                }
            }
        }
        lastState = newState;
    }

    lastState.ActionTable.AddReduce(nonTerminal.Name, nonTerminal.Rule);

    foreach (var state in states)
    {
        var nt = state.NonTerminal;
        if (nt != null)
        {
            ResolveNonTerminal(state, nt);
        }
    }

    return firstState;
}

private void ResolveNonTerminal(ParserState previousState, NonTerminal nonTerminal)
{
    var transitionState = NonTerminalTransitionStates.FirstOrDefault(x => x.NonTerminal == nonTerminal);

    if (transitionState == null)
    {
        var st = ParseNonTerminal(nonTerminal);
        transitionState = new NonTerminalTransitionState(
            nonTerminal,
            nonTerminal.Rule.EnumerateConcatenated().First(),
            st);
    }

    previousState.ActionTable.AddTransition(transitionState.Expression, transitionState.State);
}









































var queue = currentNonTerminal.Rule
        .EnumerateConcatenated()
        .ToQueue();

ParserState initialState = null;
ParserState lastState = null;
ParserState currentState = null;
            while(queue.Count > 0)
            {
                var node = queue.Dequeue();

var terminal = node as Terminal;
                if(terminal != null)
                {
                    lastState = currentState;
                    currentState = CreateState();
                    if (lastState != null)
                    {
                        lastState.ActionTable.AddTransition(terminal, currentState);
                    }
                    if (initialState == null)
                    {
                        initialState = currentState;
                    }
                }
                else
                {
                    var nonTerminal = node as NonTerminal;
                    if (nonTerminal != null)
                    {
                        lastState = currentState;
                        currentState = CreateState();
                        if(lastState != null)
                        {
                            lastState.ActionTable.AddTransition(nonTerminal, currentState);
                        }
                        if (initialState == null)
                        {
                            initialState = currentState;
                        }
                    }
                    else
                    {
                        
                    }
                }
            }


            //NonTerminalTransitionStates.Add(new NonTerminalTransitionState(currentNonTerminal, initialState))