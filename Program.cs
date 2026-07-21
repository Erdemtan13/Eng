using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

class Program
{
    static Dictionary<string, object> variables =
        new Dictionary<string, object>(
            StringComparer.OrdinalIgnoreCase
        );

    static string lastAnswer = "";

    static void Main()
    {
        if (!File.Exists("Program.eng"))
        {
            Console.WriteLine(
                "Program.eng was not found."
            );

            return;
        }

        string[] lines =
            File.ReadAllLines("Program.eng");

        RunBlock(
            lines,
            0,
            lines.Length,
            -1
        );
    }

    static void RunBlock(
        string[] lines,
        int start,
        int end,
        int parentIndentation)
    {
        int i = start;

        while (i < end)
        {
            string originalLine =
                lines[i];

            if (
                string.IsNullOrWhiteSpace(
                    originalLine
                )
            )
            {
                i++;
                continue;
            }

            int indentation =
                GetIndentation(
                    originalLine
                );

            if (
                parentIndentation != -1
                &&
                indentation <= parentIndentation
            )
            {
                return;
            }

            string line =
                originalLine.Trim();

            // ==========================================
            // EXPLICIT COMMENTS
            // ==========================================

            if (
                line.StartsWith("#")
                ||
                line.StartsWith("//")
            )
            {
                i++;
                continue;
            }

            // ==========================================
            // LOWERCASE COMMENTS
            //
            // A lowercase line is a comment UNLESS
            // it is a variable assignment.
            // ==========================================

            bool isAssignment =
                line.Contains(
                    " is ",
                    StringComparison.OrdinalIgnoreCase
                );

            if (
                char.IsLower(line[0])
                &&
                !isAssignment
            )
            {
                i++;
                continue;
            }

            // ==========================================
            // WHEN STARTED
            // ==========================================

            if (
                line.Equals(
                    "When started:",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                i++;
                continue;
            }

            // ==========================================
            // REPEAT X TIMES
            // ==========================================

            if (
                line.StartsWith(
                    "Repeat ",
                    StringComparison.OrdinalIgnoreCase
                )
                &&
                line.EndsWith(
                    " times:",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                string countText =
                    line.Substring(
                        7,
                        line.Length - 7 - 7
                    ).Trim();

                object countValue =
                    EvaluateExpression(
                        countText
                    );

                int repeatCount =
                    Convert.ToInt32(
                        countValue
                    );

                int blockStart =
                    i + 1;

                int blockEnd =
                    FindBlockEnd(
                        lines,
                        blockStart,
                        indentation
                    );

                for (
                    int repeat = 0;
                    repeat < repeatCount;
                    repeat++
                )
                {
                    RunBlock(
                        lines,
                        blockStart,
                        blockEnd,
                        indentation
                    );
                }

                i =
                    blockEnd;

                continue;
            }

            // ==========================================
            // EXECUTE NORMAL LINE
            // ==========================================

            ExecuteLine(line);

            i++;
        }
    }

    static int FindBlockEnd(
        string[] lines,
        int start,
        int parentIndentation)
    {
        int i = start;

        while (i < lines.Length)
        {
            if (
                string.IsNullOrWhiteSpace(
                    lines[i]
                )
            )
            {
                i++;
                continue;
            }

            int indentation =
                GetIndentation(
                    lines[i]
                );

            if (
                indentation <=
                parentIndentation
            )
            {
                break;
            }

            i++;
        }

        return i;
    }

    static int GetIndentation(
        string line)
    {
        int indentation = 0;

        foreach (
            char character in line
        )
        {
            if (character == ' ')
            {
                indentation++;
            }
            else if (character == '\t')
            {
                indentation += 4;
            }
            else
            {
                break;
            }
        }

        return indentation;
    }

    static void ExecuteLine(
        string line)
    {
        line =
            RemoveFinalPeriod(
                line
            );

        // ==========================================
        // ASK THE USER AND PUT THE ANSWER IN
        // ==========================================

        string askPhrase =
            "Ask the user ";

        string answerPhrase =
            " and put the answer in ";

        if (
            line.StartsWith(
                askPhrase,
                StringComparison.OrdinalIgnoreCase
            )
            &&
            line.Contains(
                answerPhrase,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            int answerPosition =
                line.IndexOf(
                    answerPhrase,
                    StringComparison.OrdinalIgnoreCase
                );

            string question =
                line.Substring(
                    askPhrase.Length,
                    answerPosition
                    -
                    askPhrase.Length
                ).Trim();

            string variableName =
                line.Substring(
                    answerPosition
                    +
                    answerPhrase.Length
                ).Trim();

            question =
                RemoveQuotes(
                    question
                );

            Console.Write(
                question + " "
            );

            lastAnswer =
                Console.ReadLine()
                ??
                "";

            variables[
                variableName
            ] =
                lastAnswer;

            return;
        }

        // ==========================================
        // ASK THE USER
        // ==========================================

        if (
            line.StartsWith(
                "Ask the user ",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            string question =
                line.Substring(
                    13
                ).Trim();

            question =
                RemoveQuotes(
                    question
                );

            Console.Write(
                question + " "
            );

            lastAnswer =
                Console.ReadLine()
                ??
                "";

            return;
        }

        // ==========================================
        // PUT THE ANSWER IN
        // ==========================================

        if (
            line.StartsWith(
                "Put the answer in ",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            string variableName =
                line.Substring(
                    19
                ).Trim();

            variables[
                variableName
            ] =
                lastAnswer;

            return;
        }

        // ==========================================
        // CONVERT TEXT TO NUMBER
        // ==========================================

        string convertStart =
            "Convert text ";

        string convertEnd =
            " to number";

        if (
            line.StartsWith(
                convertStart,
                StringComparison.OrdinalIgnoreCase
            )
            &&
            line.EndsWith(
                convertEnd,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            string variableName =
                line.Substring(
                    convertStart.Length,
                    line.Length
                    -
                    convertStart.Length
                    -
                    convertEnd.Length
                ).Trim();

            if (
                !variables.ContainsKey(
                    variableName
                )
            )
            {
                Console.WriteLine(
                    "Unknown variable: "
                    +
                    variableName
                );

                return;
            }

            string text =
                variables[
                    variableName
                ]?.ToString()
                ??
                "";

            if (
                double.TryParse(
                    text,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double number
                )
            )
            {
                variables[
                    variableName
                ] =
                    number;
            }
            else
            {
                Console.WriteLine(
                    "Could not convert "
                    +
                    variableName
                    +
                    " to a number."
                );
            }

            return;
        }

        // ==========================================
        // SAY
        // ==========================================

        if (
            line.StartsWith(
                "Say ",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            string expression =
                line.Substring(
                    4
                ).Trim();

            object result =
                EvaluateExpression(
                    expression
                );

            Console.WriteLine(
                result
            );

            return;
        }

        // ==========================================
        // VARIABLE DECLARATION
        // ==========================================

        if (
            line.StartsWith(
                "This is ",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            string variableName =
                line.Substring(
                    8
                ).Trim();

            if (
                !variables.ContainsKey(
                    variableName
                )
            )
            {
                variables[
                    variableName
                ] =
                    null;
            }

            return;
        }

        // ==========================================
        // VARIABLE ASSIGNMENT
        // ==========================================

        int isPosition =
            line.IndexOf(
                " is ",
                StringComparison.OrdinalIgnoreCase
            );

        if (
            isPosition != -1
        )
        {
            string variableName =
                line.Substring(
                    0,
                    isPosition
                ).Trim();

            string expression =
                line.Substring(
                    isPosition + 4
                ).Trim();

            object value =
                EvaluateExpression(
                    expression
                );

            variables[
                variableName
            ] =
                value;

            return;
        }

        // ==========================================
        // UNKNOWN COMMAND
        // ==========================================

        Console.WriteLine(
            "Unknown Eng command: "
            +
            line
        );
    }

    static object EvaluateExpression(
        string expression)
    {
        expression =
            expression.Trim();

        // ==========================================
        // STRING
        // ==========================================

        if (
            expression.StartsWith("\"")
            &&
            expression.EndsWith("\"")
        )
        {
            return expression.Substring(
                1,
                expression.Length - 2
            );
        }

        // ==========================================
        // VARIABLE
        // ==========================================

        if (
            variables.ContainsKey(
                expression
            )
        )
        {
            return variables[
                expression
            ];
        }

        // ==========================================
        // NUMBER
        // ==========================================

        if (
            double.TryParse(
                expression,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double number
            )
        )
        {
            return number;
        }

        // ==========================================
        // PLUS
        // ==========================================

        int plusPosition =
            expression.IndexOf(
                " plus ",
                StringComparison.OrdinalIgnoreCase
            );

        if (
            plusPosition != -1
        )
        {
            object left =
                EvaluateExpression(
                    expression.Substring(
                        0,
                        plusPosition
                    )
                );

            object right =
                EvaluateExpression(
                    expression.Substring(
                        plusPosition + 6
                    )
                );

            return Add(
                left,
                right
            );
        }

        // ==========================================
        // AND
        // ==========================================

        int andPosition =
            expression.IndexOf(
                " and ",
                StringComparison.OrdinalIgnoreCase
            );

        if (
            andPosition != -1
        )
        {
            object left =
                EvaluateExpression(
                    expression.Substring(
                        0,
                        andPosition
                    )
                );

            object right =
                EvaluateExpression(
                    expression.Substring(
                        andPosition + 5
                    )
                );

            return Add(
                left,
                right
            );
        }

        // ==========================================
        // MINUS
        // ==========================================

        int minusPosition =
            expression.IndexOf(
                " minus ",
                StringComparison.OrdinalIgnoreCase
            );

        if (
            minusPosition != -1
        )
        {
            double left =
                Convert.ToDouble(
                    EvaluateExpression(
                        expression.Substring(
                            0,
                            minusPosition
                        )
                    )
                );

            double right =
                Convert.ToDouble(
                    EvaluateExpression(
                        expression.Substring(
                            minusPosition + 7
                        )
                    )
                );

            return left - right;
        }

        // ==========================================
        // MULTIPLIED BY
        // ==========================================

        int multipliedPosition =
            expression.IndexOf(
                " multiplied by ",
                StringComparison.OrdinalIgnoreCase
            );

        if (
            multipliedPosition != -1
        )
        {
            double left =
                Convert.ToDouble(
                    EvaluateExpression(
                        expression.Substring(
                            0,
                            multipliedPosition
                        )
                    )
                );

            double right =
                Convert.ToDouble(
                    EvaluateExpression(
                        expression.Substring(
                            multipliedPosition + 15
                        )
                    )
                );

            return left * right;
        }

        // ==========================================
        // DIVIDED BY
        // ==========================================

        int dividedPosition =
            expression.IndexOf(
                " divided by ",
                StringComparison.OrdinalIgnoreCase
            );

        if (
            dividedPosition != -1
        )
        {
            double left =
                Convert.ToDouble(
                    EvaluateExpression(
                        expression.Substring(
                            0,
                            dividedPosition
                        )
                    )
                );

            double right =
                Convert.ToDouble(
                    EvaluateExpression(
                        expression.Substring(
                            dividedPosition + 12
                        )
                    )
                );

            if (
                right == 0
            )
            {
                throw new Exception(
                    "You cannot divide by zero."
                );
            }

            return left / right;
        }

        return expression;
    }

    static object Add(
        object left,
        object right)
    {
        if (
            left is string
            ||
            right is string
        )
        {
            return Convert.ToString(
                left
            )
            +
            Convert.ToString(
                right
            );
        }

        double leftNumber =
            Convert.ToDouble(
                left
            );

        double rightNumber =
            Convert.ToDouble(
                right
            );

        return leftNumber
            +
            rightNumber;
    }

    static string RemoveFinalPeriod(
        string line)
    {
        if (
            line.EndsWith(".")
        )
        {
            return line.Substring(
                0,
                line.Length - 1
            ).Trim();
        }

        return line;
    }

    static string RemoveQuotes(
        string text)
    {
        text =
            text.Trim();

        if (
            text.StartsWith("\"")
            &&
            text.EndsWith("\"")
        )
        {
            return text.Substring(
                1,
                text.Length - 2
            );
        }

        return text;
    }
}