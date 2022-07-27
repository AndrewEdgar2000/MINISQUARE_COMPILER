using static System.Console;

namespace Compiler.IO
{
    /// <summary>
    /// An object for reporting errors in the compilation process
    /// </summary>
    public class ErrorReporter
    {
        /// <summary>
        /// Whether or not any errors have been encountered
        /// </summary>

        //The number of errors in the program
        public int NumErrors { get; set; } = 0;


        //Whether or not any errors have been encountered
        public bool HasErrors
        {
            get
            {
                return NumErrors > 0;
            }
        }

        //The position
        public Position Position { get; }


        public void ErrorReport(string mess, Position position)
        {
            NumErrors += 1;
            WriteLine(mess + "\n" +  $"Position of error found: {position}");
        }
    }
}