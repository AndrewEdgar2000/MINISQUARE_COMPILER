﻿using Compiler.IO;
using Compiler.Nodes;
using Compiler.Nodes.CommandNodes;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace Compiler.SemanticAnalysis
{
    /// <summary>
    /// A type checker
    /// </summary>
    public class TypeChecker
    {
        /// <summary>
        /// The error reporter
        /// </summary>
        public ErrorReporter Reporter { get; }

        /// <summary>
        /// Creates a new type checker
        /// </summary>
        /// <param name="reporter">The error reporter to use</param>
        public TypeChecker(ErrorReporter reporter)
        {
            Reporter = reporter;
        }

        /// <summary>
        /// Carries out type checking on a program
        /// </summary>
        /// <param name="tree">The program to check</param>
        public void PerformTypeChecking(ProgramNode tree)
        {
            PerformTypeCheckingOnProgram(tree);
        }

        /// <summary>
        /// Carries out type checking on a node
        /// </summary>
        /// <param name="node">The node to perform type checking on</param>
        private void PerformTypeChecking(IAbstractSyntaxTreeNode node)
        {
            if (node is null)
                // Shouldn't have null nodes - there is a problem with your parsing
                Debugger.Write("Tried to perform type checking on a null tree node");
            else if (node is ErrorNode)
                // Shouldn't have error nodes - there is a problem with your parsing
                Debugger.Write("Tried to perform type checking on an error tree node");
            else
            {
                string functionName = "PerformTypeCheckingOn" + node.GetType().Name.Remove(node.GetType().Name.Length - 4);
                MethodInfo function = this.GetType().GetMethod(functionName, NonPublic | Public | Instance | Static);
                if (function == null)
                    // There is not a correctly named function below
                    Debugger.Write($"Couldn't find the function {functionName} when type checking");
                else
                    function.Invoke(this, new[] { node });
            }
        }



        /// <summary>
        /// Carries out type checking on a program node
        /// </summary>
        /// <param name="programNode">The node to perform type checking on</param>
        private void PerformTypeCheckingOnProgram(ProgramNode programNode)
        {
            PerformTypeChecking(programNode.Command);
        }


        //Carries out type checking for For command
        private void PerformTypeCheckingOnForCommand(ForCommandNode forCommandNode)
        {
            PerformTypeChecking(forCommandNode.expressionNode);
            PerformTypeChecking(forCommandNode.command1);
            PerformTypeChecking(forCommandNode.command2);
            PerformTypeChecking(forCommandNode.command3);
            if (forCommandNode.expressionNode.Type != StandardEnvironment.BooleanType)
            {
                Position position = forCommandNode.Position;
                Reporter.ErrorReport("Function takes no arguments but is called with one", position);
            }
        }

        //Carries out type checking for a forever command
        private void PerformTypeCheckingOnForeverCommand(ForeverCommandNode foreverCommandNode)
        {
            PerformTypeChecking(foreverCommandNode.commandNode);
        }


        /// <summary>
        /// Carries out type checking on an assign command node
        /// </summary>
        /// <param name="assignCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnAssignCommand(AssignCommandNode assignCommand)
        {
            PerformTypeChecking(assignCommand.Identifier);
            PerformTypeChecking(assignCommand.Expression);
            if (!(assignCommand.Identifier.Declaration is IVariableDeclarationNode varDeclaration))
            {
                Position position = assignCommand.Position;
                // Error - identifier is not a variable
                Reporter.ErrorReport("Identifier is not a variable", position);
            }
            else if (varDeclaration.EntityType != assignCommand.Expression.Type)
            {
                Position position = varDeclaration.Position;
                // Error - expression is wrong type for the variable
                Reporter.ErrorReport("Expression is wrong type for the variable", position);
            }
        }

        /// <summary>
        /// Carries out type checking on a blank command node
        /// </summary>
        /// <param name="blankCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnBlankCommand(BlankCommandNode blankCommand)
        {
        }

        /// <summary>
        /// Carries out type checking on a call command node
        /// </summary>
        /// <param name="callCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnCallCommand(CallCommandNode callCommand)
        {
            PerformTypeChecking(callCommand.Identifier);
            PerformTypeChecking(callCommand.Parameter);
            if (!(callCommand.Identifier.Declaration is FunctionDeclarationNode functionDeclaration))
            {
                Position position = callCommand.Position;
                // Error: Identifier is not a function
                Reporter.ErrorReport("Identifier is not a function", position);
            }
            else if (GetNumberOfArguments(functionDeclaration.Type) == 0)
            {
                if (!(callCommand.Parameter is BlankParameterNode))
                {
                    // Error: function takes no arguments but is called with one
                    Position position = callCommand.Position;
                    // Error: function takes no arguments but is called with one
                    Reporter.ErrorReport("Function takes no arguments but is called with one", position);
                }
            }
            else
            {
                if (callCommand.Parameter is BlankParameterNode)
                {
                    Position position = callCommand.Position;
                    // Error: function takes an argument but is called without one
                    Reporter.ErrorReport("Function takes an argument but is called without one", position);
                }
                else
                {
                    if (GetArgumentType(functionDeclaration.Type, 0) != callCommand.Parameter.Type)
                    {
                        Position position = functionDeclaration.Position;
                        // Error: Function called with parameter of the wrong type
                        Reporter.ErrorReport("Function called with parameter of the wrong type", position);
                    }
                    if (ArgumentPassedByReference(functionDeclaration.Type, 0))
                    {
                        if (!(callCommand.Parameter is VarParameterNode))
                        {
                            Position position = callCommand.Position;
                            // Error: Function requires a var parameter but has been given an expression parameter
                            Reporter.ErrorReport("Function requires a var parameter but has been given an expression parameter", position);
                        }
                    }
                    else
                    {
                        if (!(callCommand.Parameter is ExpressionParameterNode))
                        {

                            // Error: Function requires an expression parameter but has been given a var parameter
                            Position position = callCommand.Position;
                            Reporter.ErrorReport("Function requires an expression parameter but has been given a var parameter ", position);

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Carries out type checking on an if command node
        /// </summary>
        /// <param name="ifCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIfCommand(IfCommandNode ifCommand)
        {
            PerformTypeChecking(ifCommand.Expression);
            PerformTypeChecking(ifCommand.ThenCommand);
            PerformTypeChecking(ifCommand.ElseCommand);
            if (ifCommand.Expression.Type != StandardEnvironment.BooleanType)
            {
                Position position = ifCommand.Position;
                // Error: expression needs to be a boolean
                Reporter.ErrorReport("Expression needs to be a boolean (If)", position);
            }
        }

        /// <summary>
        /// Carries out type checking on a let command node
        /// </summary>
        /// <param name="letCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnLetCommand(LetCommandNode letCommand)
        {
            PerformTypeChecking(letCommand.Declaration);
            PerformTypeChecking(letCommand.Command);
        }

        /// <summary>
        /// Carries out type checking on a sequential command node
        /// </summary>
        /// <param name="sequentialCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnSequentialCommand(SequentialCommandNode sequentialCommand)
        {
            foreach (ICommandNode command in sequentialCommand.Commands)
                PerformTypeChecking(command);
        }

        /// <summary>
        /// Carries out type checking on a while command node
        /// </summary>
        /// <param name="whileCommand">The node to perform type checking on</param>
        private void PerformTypeCheckingOnWhileCommand(WhileCommandNode whileCommand)
        {
            PerformTypeChecking(whileCommand.Expression);
            PerformTypeChecking(whileCommand.Command);
            if (whileCommand.Expression.Type != StandardEnvironment.BooleanType)
            {
                // Error: expression needs to be a boolean
                Position position = whileCommand.Position;
                Reporter.ErrorReport("Expression needs to be a boolean (While)", position);
            }
        }



        /// <summary>
        /// Carries out type checking on a const declaration node
        /// </summary>
        /// <param name="constDeclaration"The node to perform type checking on></param>
        private void PerformTypeCheckingOnConstDeclaration(ConstDeclarationNode constDeclaration)
        {
            PerformTypeChecking(constDeclaration.Identifier);
            PerformTypeChecking(constDeclaration.Expression);
        }

        /// <summary>
        /// Carries out type checking on a sequential declaration node
        /// </summary>
        /// <param name="sequentialDeclaration">The node to perform type checking on</param>
        private void PerformTypeCheckingOnSequentialDeclaration(SequentialDeclarationNode sequentialDeclaration)
        {
            foreach (IDeclarationNode declaration in sequentialDeclaration.Declarations)
                PerformTypeChecking(declaration);
        }

        /// <summary>
        /// Carries out type checking on a var declaration node
        /// </summary>
        /// <param name="varDeclaration">The node to perform type checking on</param>
        private void PerformTypeCheckingOnVarDeclaration(VarDeclarationNode varDeclaration)
        {
            PerformTypeChecking(varDeclaration.TypeDenoter);
            PerformTypeChecking(varDeclaration.Identifier);
        }



        /// <summary>
        /// Carries out type checking on a binary expression node
        /// </summary>
        /// <param name="binaryExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnBinaryExpression(BinaryExpressionNode binaryExpression)
        {
            PerformTypeChecking(binaryExpression.Op);
            PerformTypeChecking(binaryExpression.LeftExpression);
            PerformTypeChecking(binaryExpression.RightExpression);
            if (!(binaryExpression.Op.Declaration is BinaryOperationDeclarationNode opDeclaration))
            {
                // Error: operator is not a binary operator
                Position position = binaryExpression.Position;
                Reporter.ErrorReport("Operator is not a binary operator", position);
            }
            else
            {
                if (GetArgumentType(opDeclaration.Type, 0) == StandardEnvironment.AnyType)
                {
                    if (binaryExpression.LeftExpression.Type != binaryExpression.RightExpression.Type)
                    {
                        // Error: left and right hand side arguments not the same type
                        Position position = binaryExpression.Position;
                        Reporter.ErrorReport("Left and right hand side arguments not the same type", position);
                    }
                }
                else
                {
                    if (GetArgumentType(opDeclaration.Type, 0) != binaryExpression.LeftExpression.Type)
                    {
                        // Error: Left hand expression is wrong type
                        Position position = binaryExpression.Position;
                        Reporter.ErrorReport("Left hand expression is wrong type", position);
                    }
                    if (GetArgumentType(opDeclaration.Type, 1) != binaryExpression.RightExpression.Type)
                    {
                        // Error: Right hand expression is wrong type
                        Position position = binaryExpression.Position;
                        Reporter.ErrorReport("Right hand expression is wrong type", position);
                    }
                }
                binaryExpression.Type = GetReturnType(opDeclaration.Type);
            }
        }

        /// <summary>
        /// Carries out type checking on a character expression node
        /// </summary>
        /// <param name="characterExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnCharacterExpression(CharacterExpressionNode characterExpression)
        {
            PerformTypeChecking(characterExpression.CharLit);
            characterExpression.Type = StandardEnvironment.CharType;
        }

        /// <summary>
        /// Carries out type checking on an ID expression node
        /// </summary>
        /// <param name="idExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIdExpression(IdExpressionNode idExpression)
        {
            PerformTypeChecking(idExpression.Identifier);
            if (!(idExpression.Identifier.Declaration is IEntityDeclarationNode declaration))
            {
                // Error: identifier is not a variable or constant
                Position position = idExpression.Position;
                Reporter.ErrorReport("Identifier is not a variable or constant", position);
            }
            else
                idExpression.Type = declaration.EntityType;
        }

        /// <summary>
        /// Carries out type checking on a  node
        /// </summary>
        /// <param name="integerExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIntegerExpression(IntegerExpressionNode integerExpression)
        {
            PerformTypeChecking(integerExpression.IntLit);
            integerExpression.Type = StandardEnvironment.IntegerType;
        }

        /// <summary>
        /// Carries out type checking on a unary expression node
        /// </summary>
        /// <param name="unaryExpression">The node to perform type checking on</param>
        private void PerformTypeCheckingOnUnaryExpression(UnaryExpressionNode unaryExpression)
        {
            PerformTypeChecking(unaryExpression.Op);
            PerformTypeChecking(unaryExpression.Expression);
            if (!(unaryExpression.Op.Declaration is UnaryOperationDeclarationNode opDeclaration))
            {
                // Error: operator is not a unary operator
                Position position = unaryExpression.Position;
                Reporter.ErrorReport("Is not a unary operator", position);
            }
            else
            {
                if (GetArgumentType(opDeclaration.Type, 0) != unaryExpression.Expression.Type)
                {
                    // Error: expression is the wrong type
                    Position position = opDeclaration.Position;
                    Reporter.ErrorReport("Expression is the wrong type", position);
                }
                unaryExpression.Type = GetReturnType(opDeclaration.Type);
            }
        }



        /// <summary>
        /// Carries out type checking on a blank parameter
        /// </summary>
        /// <param name="blankParameter">The node to perform type checking on</param>
        private void PerformTypeCheckingOnBlankParameter(BlankParameterNode blankParameter)
        {
        }

        /// <summary>
        /// Carries out type checking on an expression parameter node
        /// </summary>
        /// <param name="expressionParameter">The node to perform type checking on</param>
        private void PerformTypeCheckingOnExpressionParameter(ExpressionParameterNode expressionParameter)
        {
            PerformTypeChecking(expressionParameter.Expression);
            expressionParameter.Type = expressionParameter.Expression.Type;
        }

        /// <summary>
        /// Carries out type checking on a var parameter node
        /// </summary>
        /// <param name="varParameter">The node to perform type checking on</param>
        private void PerformTypeCheckingOnVarParameter(VarParameterNode varParameter)
        {
            PerformTypeChecking(varParameter.Identifier);
            if (!(varParameter.Identifier.Declaration is IVariableDeclarationNode varDeclaration))
            {
                // Error: identifier is not a variable
                Position position = varParameter.Position;
                Reporter.ErrorReport("Identifier is not a variable", position);
            }
            else
                varParameter.Type = varDeclaration.EntityType;
        }



        /// <summary>
        /// Carries out type checking on a type denoter node
        /// </summary>
        /// <param name="typeDenoter">The node to perform type checking on</param>
        private void PerformTypeCheckingOnTypeDenoter(TypeDenoterNode typeDenoter)
        {
            PerformTypeChecking(typeDenoter.Identifier);
            if (!(typeDenoter.Identifier.Declaration is SimpleTypeDeclarationNode declaration))
            {
                // Error: identifier is not a type
                Position position = typeDenoter.Position;
                Reporter.ErrorReport("Identifier is not a type", position);
            }
            else
                typeDenoter.Type = declaration;
        }



        /// <summary>
        /// Carries out type checking on a character literal node
        /// </summary>
        /// <param name="characterLiteral">The node to perform type checking on</param>
        private void PerformTypeCheckingOnCharacterLiteral(CharacterLiteralNode characterLiteral)
        {
            if (characterLiteral.Value < short.MinValue || characterLiteral.Value > short.MaxValue)
            {
                // Error - value too big         
                Position position = characterLiteral.Position;
                Reporter.ErrorReport("Value too big", position);
            }
        }

        /// <summary>
        /// Carries out type checking on an identifier node
        /// </summary>
        /// <param name="identifier">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIdentifier(IdentifierNode identifier)
        {
        }

        /// <summary>
        /// Carries out type checking on an integer literal node
        /// </summary>
        /// <param name="integerLiteral">The node to perform type checking on</param>
        private void PerformTypeCheckingOnIntegerLiteral(IntegerLiteralNode integerLiteral)
        {
            if (integerLiteral.Value < short.MinValue || integerLiteral.Value > short.MaxValue)
            {
                // Error - value too big
                Position position = integerLiteral.Position;
                Reporter.ErrorReport("Value too big", position);
            }
        }

        /// <summary>
        /// Carries out type checking on an operation node
        /// </summary>
        /// <param name="operation">The node to perform type checking on</param>
        private void PerformTypeCheckingOnOperator(OperatorNode operation)
        {
        }



        /// <summary>
        /// Gets the number of arguments that a function takes
        /// </summary>
        /// <param name="node">The function</param>
        /// <returns>The number of arguments taken by the function</returns>
        private static int GetNumberOfArguments(FunctionTypeDeclarationNode node)
        {
            return node.Parameters.Length;
        }

        /// <summary>
        /// Gets the type of a function's argument
        /// </summary>
        /// <param name="node">The function</param>
        /// <param name="argument">The index of the argument</param>
        /// <returns>The type of the given argument to the function</returns>
        private static SimpleTypeDeclarationNode GetArgumentType(FunctionTypeDeclarationNode node, int argument)
        {
            return node.Parameters[argument].type;
        }

        /// <summary>
        /// Gets the whether an argument to a function is passed by reference
        /// </summary>
        /// <param name="node">The function</param>
        /// <param name="argument">The index of the argument</param>
        /// <returns>True if and only if the argument is passed by reference</returns>
        private static bool ArgumentPassedByReference(FunctionTypeDeclarationNode node, int argument)
        {
            return node.Parameters[argument].byRef;
        }

        /// <summary>
        /// Gets the return type of a function
        /// </summary>
        /// <param name="node">The function</param>
        /// <returns>The return type of the function</returns>
        private static SimpleTypeDeclarationNode GetReturnType(FunctionTypeDeclarationNode node)
        {
            return node.ReturnType;
        }
    }
}