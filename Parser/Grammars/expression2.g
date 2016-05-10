Expr = Term
	 | Term, "+", Term
	 | Term, "-", Term
	 ;

Term = Factor
	 | Factor, "*", Factor
	 | Factor, "/", Factor
	 ;

Factor = RealNum
	   | "-", RealNum
	   | "(", Expr, ")"
	   ;

RealNum = "1" | "2" ;

grammar = Expr;