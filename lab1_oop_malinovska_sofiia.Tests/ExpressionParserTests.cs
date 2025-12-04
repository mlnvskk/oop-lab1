using System;
using System.Collections.Generic;
using lab1_oop_malinovska_sofiia.Parser;
using Xunit;

namespace lab1_oop_malinovska_sofiia.Tests
{
    public class ExpressionParserTests
    {
        private static double Eval(string expr, Dictionary<string, double>? cells = null)
        {
            cells ??= new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            double Resolver(string name)
            {
                return cells.TryGetValue(name, out var value) ? value : 0.0;
            }

            var parser = new ExpressionParser(expr, Resolver);
            return parser.Parse();
        }


        [Fact]
        public void SimpleArithmetic_ReturnsCorrectResult()
        {
          
            var result = Eval("10 + 2 * 6 / (4 - 1)");

            Assert.Equal(14.0, result, 3); 
        }

        [Theory]
        [InlineData("10/2", 5)]
        [InlineData("2+2*2", 6)]
        [InlineData("(2+2)*2", 8)]
        [InlineData("5-8+3", 0)]
        [InlineData("2^3", 8)]
        public void VariousArithmeticExpressions_ReturnExpected(string expr, double expected)
        {
            var result = Eval(expr);

            Assert.Equal(expected, result, 3);
        }

        [Theory]
        [InlineData("inc(5)", 6)]
        [InlineData("dec(5)", 4)]
        [InlineData("10 mod 3", 1)]
        [InlineData("10 div 3", 3)]
        public void FunctionsAndIntegerOps_ReturnExpected(string expr, double expected)
        {
            var result = Eval(expr);

            Assert.Equal(expected, result, 3);
        }
        

        [Fact]
        public void Expression_WithCellReferences_ReturnsCorrect()
        {
            var cells = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["A1"] = 10,
                ["B2"] = 20,
                ["C3"] = 5
            };

            var result = Eval("A1 + B2 - 2*3", cells); 

            Assert.Equal(24.0, result, 3);
        }

        [Theory]
        [InlineData("(A1*2)+B2", 40)]    
        [InlineData("B2/C3", 4)]         
        [InlineData("A1+B2+C3", 35)]     
        [InlineData("A1*C3-B2", 30)]     
        public void VariousFormulas_WithCells_ReturnExpected(string expr, double expected)
        {
            var cells = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["A1"] = 10,
                ["B2"] = 20,
                ["C3"] = 5
            };

            var result = Eval(expr, cells);

            Assert.Equal(expected, result, 3);
        }

        [Fact]
        public void UnknownCell_ReturnsZero()
        {
            var cells = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                ["A1"] = 10
            };

            var result = Eval("D4 + A1", cells);

            Assert.Equal(10.0, result, 3);
        }

    }
}
