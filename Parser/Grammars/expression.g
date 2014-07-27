id = "A" | "B" | "C" ;
int = "1" | "2" | "3" ;

Value = int
	| id ;

Products = Products, "*", Value 
	| Value ;

Sums = Sums, "+", Products
	| Products ;

Assign = id, "=", Sums ;

Grammar = Assign ;