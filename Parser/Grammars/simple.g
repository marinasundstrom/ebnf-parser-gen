int = "1" | "2" | "3" ;

f = "(",  e, ")" | int ;

t = t, "*", f 
	| f ;
	
e = e, "+", t 
	| t ;

grammar = e ;