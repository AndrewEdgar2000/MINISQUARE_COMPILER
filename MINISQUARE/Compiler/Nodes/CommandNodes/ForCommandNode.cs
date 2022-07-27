using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Nodes.CommandNodes
{
    public class ForCommandNode : ICommandNode
    {
        //The Position 
        public Position Position { get; }

        //Expression Node
        public IExpressionNode expressionNode { get; }

        //Three Command node interfaces
        public ICommandNode command1 { get; }
        public ICommandNode command2 { get; }
        public ICommandNode command3 { get; }


        //Creates a For command Node
        public ForCommandNode(Position pos, IExpressionNode ex, ICommandNode c1, ICommandNode c2, ICommandNode c3)
        {
            Position = pos;
            expressionNode = ex;
            command1 = c1;
            command2 = c2;
            command3 = c3;
        }
    }
}
