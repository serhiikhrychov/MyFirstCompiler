using System.IO;
using System.Net;
using Antlr4.Runtime;
using compiler;

var fileName = "Content\\prog.lg";

var fileContext = File.ReadAllText(fileName);

AntlrInputStream inputStream = new AntlrInputStream(fileContext);
var langLexer = new LangLexer(inputStream);
// langLexer.RemoveErrorListeners();
CommonTokenStream commonTokenStream = new CommonTokenStream(langLexer);
var langParser = new LangParser(commonTokenStream);
var langContext = langParser.program();
var visitor = new LangVisitor();        
visitor.Visit(langContext);