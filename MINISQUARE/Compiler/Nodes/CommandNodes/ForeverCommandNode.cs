using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Nodes.CommandNodes
{
    public class ForeverCommandNode : ICommandNode
    {
        //The position
        public Position Position { get; }

        //The command Node
        public ICommandNode commandNode { get; }

        public IExpressionNode expression { get; }

        //Creates a forever command node
        public ForeverCommandNode(Position pos, ICommandNode command, IExpressionNode exp)
        {
            Position = pos;
            commandNode = command;
            expression = exp;
        }
    }
}
