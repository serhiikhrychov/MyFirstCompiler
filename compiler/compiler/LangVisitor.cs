using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace compiler;

public class LangVisitor : LangBaseVisitor<object?>
{
    private Dictionary<string, object?> Variables { get; } = new();

    public LangVisitor()
    {
       Variables["Pow"] = new Func<object[], object>(Pow);
    }
    private object Pow(object[] args)
    {
       return Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]));
    }

    public override object? VisitFunctionCall(LangParser.FunctionCallContext context)
    {
        var name = context.IDENTIFIER().GetText();
        var args = context.expression().Select(Visit).ToArray();
        
        if (!Variables.ContainsKey(name))
        {
            throw new Exception($"Function {name} is not defined");
        }

        if (Variables[name] is not Func<object?[], object?> func)
        {
            throw new Exception($"Function {name} is not a function");
        }

        return func(args);
    }

    public override object? VisitAssignment(LangParser.AssignmentContext context)
    {
      var varName = context.IDENTIFIER().GetText();
      var value = Visit(context.expression());

      Variables[varName] = value;

      return null;
    }

    public override object? VisitIdentifierExpression(LangParser.IdentifierExpressionContext context)
    {
        var varName = context.IDENTIFIER().GetText();

        if (!Variables.ContainsKey(varName))
        {
            throw new Exception($"Variable {varName} is not defined");
        }

        return Variables[varName];
    }

    public override object? VisitConstant(LangParser.ConstantContext context)
    {
        if (context.INTEGER() is { } i)
            return int.Parse(i.GetText());
        if (context.FLOAT() is { } f)
            return float.Parse(f.GetText());
        if (context.STRING() is { } s)
            return s.GetText().Substring(1, s.GetText().Length - 1); // from the first char to last -1 
        if (context.BOOL() is { } b)
            return b.GetText() == "true";
        if (context.NULL() is { } )
            return null;
        throw new NotImplementedException();
    }

    public override object? VisitAdditiveExpression(LangParser.AdditiveExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        var op = context.addOp().GetText();

        return op switch
        {
            "+" => Add(left, right),
            "-" => Subtract(left, right),
            _ => throw new NotImplementedException()
        };
    }

    public override object? VisitMultiplicativeExpression(LangParser.MultiplicativeExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        
        var op = context.multOp().GetText();
        
        return op switch
        {
            "*" => Multiply(left, right),
            "/" => Divide(left, right),
            _ => throw new NotImplementedException()
        };
        
    }

    private object? Multiply(object? left, object? right)
    {
        if (left is int l && right is int r)
            return l * r;
        if (left is float lf && right is float rf)
            return lf * rf;
        if (left is int lint && right is float rfloat)
            return lint * rfloat;
        if (left is float lfloat && right is int rint)
            return lfloat * rint;

        throw new Exception($"Cannot multiply values of types {left?.GetType()} and {right?.GetType()}");
    }
    
    private object? Divide(object? left, object? right)
    {
        if (left is int l && right is int r)
            return l / r;
        if (left is float lf && right is float rf)
            return lf / rf;
        if (left is int lint && right is float rfloat)
            return lint / rfloat;
        if (left is float lfloat && right is int rint)
            return lfloat / rint;

        throw new Exception($"Cannot divide values of types {left?.GetType()} and {right?.GetType()}");
    }

    private object? Add(object? left, object? right)
    {
        if (left is int l && right is int r)
            return l + r;
        if (left is float lf && right is float rf)
            return lf + rf;
        if (left is int lint && right is float rfloat)
            return lint + rfloat;
        if (left is float lfloat && right is int rint)
            return lfloat + rint;
        if (left is string || right is string)
            return $"{left}{right}";

        throw new Exception($"Cannot add values of types {left?.GetType()} and {right?.GetType()}");
    }
    
    private object? Subtract(object? left, object? right)
    {
        if (left is int l && right is int r)
            return l - r;
        if (left is float lf && right is float rf)
            return lf - rf;
        if (left is int lint && right is float rfloat)
            return lint - rfloat;
        if (left is float lfloat && right is int rint)
            return lfloat - rint;

        throw new Exception($"Cannot subscribe values of types {left?.GetType()} and {right?.GetType()}");
    }

    public override object? VisitWhileBlock(LangParser.WhileBlockContext context)
    {
        Func<object?, bool> condition = context.WHILE().GetText() == "while" ? IsTrue : IsFalse;

        if (condition(Visit(context.expression())))
        {
            do
            {
                Visit(context.block());
            } while(condition(Visit(context.expression())));
        }
        else
        {
            Visit(context.elseIfBlock());
        }
        return null;
    }

    public override object? VisitIfBlock(LangParser.IfBlockContext context)
    {
        Func<object?, bool> condition = context.IF().GetText() == "if" ? IsTrue : IsFalse;
        
        if (condition(Visit(context.expression())))
        {
            Visit(context.block());
        }
        else
        {
            Visit(context.elseIfBlock());
        }
        return null;
    }

    public override object? VisitComparisonExpression(LangParser.ComparisonExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        var op = context.compareOp().GetText();

        return op switch
        {
            "<" => LessThan(left, right),
            ">" => GreatThan(left, right),
            _ => throw new NotImplementedException()
        };
    }
    
    private bool IsTrue(object? value)
    {
        if (value is bool b)
            return b;

        throw new Exception("Value is not boolean");
    }

    private bool IsFalse(object? value) => !IsTrue(value);

    private bool LessThan(object? left, object? right)
    {
        if (left is int l && right is int r)
            return l < r;
        if (left is float lf && right is float rf)
            return lf < rf;
        if (left is int lint && right is float rfloat)
            return lint < rfloat;
        if (left is float lfloat && right is int rint)
            return lfloat < rint;

        throw new Exception($"Cannot compare values of types {left?.GetType()} and {right?.GetType()}");
    }
    
    private bool GreatThan(object? left, object? right)
    {
        if (left is int l && right is int r)
            return l > r;
        if (left is float lf && right is float rf)
            return lf > rf;
        if (left is int lint && right is float rfloat)
            return lint > rfloat;
        if (left is float lfloat && right is int rint)
            return lfloat > rint;

        throw new Exception($"Cannot compare values of types {left?.GetType()} and {right?.GetType()}");

    }
}