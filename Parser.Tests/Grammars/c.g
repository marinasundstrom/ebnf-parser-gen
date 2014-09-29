Identifier = { "A" | "B" | "C" };

NamespaceDecl = "namespace", Identifier, "{", { TypeOrNamespaceDecl } ,"}" ;

ClassMemberDecl = "bob" ;

TypeDecl = "class", Identifier, "{", { ClassMemberDecl }, "}" ;

TypeOrNamespaceDecl = TypeDecl
					| NamespaceDecl
					;

root = { TypeOrNamespaceDecl } ;