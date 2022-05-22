grammar Lang;

program: line* EOF; 

line: statement | ifBlock | whileBlock;

statement: (assignment|functionCall) ';';

ifBlock: IF expression block ('else' elseIfBlock);

IF: 'if';

elseIfBlock: block | ifBlock;

whileBlock: WHILE expression block ('else' elseIfBlock);

WHILE: 'while' | 'until';

assignment: IDENTIFIER '=' expression;

functionCall: IDENTIFIER '(' (expression (',' expression)*)? ')';

expression
    : constant                              #constantExpression
    | IDENTIFIER                            #identifierExpression
    | functionCall                          #functionalCallExpression
    | '(' expression ')'                    #parametrizedExpression
    | '!' expression                        #notExpression
    | expression multOp expression          #multiplicativeExpression
    | expression addOp expression           #additiveExpression
    | expression compareOp expression       #comparisonExpression
    | expression boolOp expression          #booleanExpression
    ;

multOp: '*' | '/' | '%';
addOp: '+' | '-';
compareOp: '==' | '!=' | '>' | '<' | '>=' | '<=';
boolOp: BOOL_OPERATOR;

BOOL_OPERATOR: '&&' | '||';
  
INTEGER: [0-9]+;
FLOAT: [0-9]+ '.' [0-9]+;
STRING: ('"' ~'"'* '"') | ('\'' ~'\''* '\'');
BOOL: 'true' | 'false';
NULL: 'null';

constant: INTEGER | FLOAT | STRING | BOOL | NULL;
WS: [ \t\r\n]+ -> skip;
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;

block: '{' line* '}';

