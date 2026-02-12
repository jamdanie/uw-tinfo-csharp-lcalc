/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Change History Form1.cs
// Date        Developer     Description
// ----------  ------------  ----------------------------------------------------
// 01/27/2026  JADANIE       Project created.
// 01/27/2026  JADANIE       Implemented initial calculator UI and basic input logic.
// 01/29/2026  JADANIE       Fixed leading zero behavior in numBtn_Click to prevent inputs like "07"
//                           and simplified logic using a conditional expression.
// 01/29/2026  JADANIE       Clarified decimal button behavior to ensure only one decimal point.
// 01/29/2026  JADANIE       Improved backspace behavior to prevent empty display; resets to "0" when
//                           the last digit is removed.
// 01/29/2026  JADANIE       Added class-level fields to store operand and operator state for operations.
// 01/29/2026  JADANIE       Identified future enhancement to display full expression
//                           during operand entry for clearer user feedback.
// 02/03/2026  JADANIE       Implemented equals (=) using stored operand/operator; added divide-by-zero handling.
// 02/05/2026  JADANIE       Added safe display parsing and centralized error recovery (MessageBox + reset)
//                           to prevent crashes and meet graceful error-handling requirements.
// 02/06/2026  JADANIE       Enhanced operator handling to allow operator replacement
//                           before entering the right operand; improved input state tracking.
// 02/06/2026  JADANIE       Added repeated equals support by storing last operator + last operand,
//                           so pressing '=' again repeats the last operation (modern calculator behavior).
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace LCalc
{
    public partial class Form1 : Form
    {
        // Stores the left-hand operand for the pending operation.
        private double leftOperand = 0.0;

        // Stores the selected operator (+, -, x, /).
        private string mathOperator = string.Empty;

        // Tracks whether an operator was just pressed and we're waiting for the next number entry.
        private bool waitingForRightOperand = false;

        // --- Repeated Equals Support ---
        // Stores the last right operand used during a completed operation.
        private double? lastOperand = null;

        // Stores the last operator used during a completed operation.
        private string lastOperator = null;

        // Returns the display value if it's a valid number, otherwise returns null.
        private double? GetDisplayValueOrNull()
        {
            if (double.TryParse(display.Text, out double value))
                return value;

            return null;
        }

        // Clears calculator state and shows an error message (rubric: recover gracefully).
        private void NotifyAndReset(string message)
        {
            MessageBox.Show(message, "Calculator", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            display.Text = "0";
            leftOperand = 0.0;
            mathOperator = string.Empty;
            waitingForRightOperand = false;

            // Reset repeated-equals memory on error so the next use is predictable.
            lastOperand = null;
            lastOperator = null;
        }

        public Form1()
        {
            InitializeComponent();
        }

        // Adds the clicked number to the display (prevents leading zero).
        private void numBtn_Click(object sender, EventArgs e)
        {
            string digit = ((Button)sender).Text;

            // If an operator was just pressed, start a new number entry.
            if (waitingForRightOperand)
            {
                display.Text = digit;
                waitingForRightOperand = false;
                return;
            }

            display.Text = (display.Text == "0") ? digit : display.Text + digit;
        }

        // Clears the current entry only (does not wipe stored leftOperand/operator).
        private void clearEntryBtn_Click(object sender, EventArgs e)
        {
            display.Text = "0";
            waitingForRightOperand = false;
        }

        // Clears the entire calculator state.
        private void clearAllBtn_Click(object sender, EventArgs e)
        {
            display.Text = "0";
            leftOperand = 0.0;
            mathOperator = string.Empty;
            waitingForRightOperand = false;

            // Clear repeated-equals memory so user is starting fresh.
            lastOperand = null;
            lastOperator = null;
        }

        // Adds a decimal point if one does not already exist.
        private void decimalPtBtn_Click(object sender, EventArgs e)
        {
            // If an operator was just pressed, start a new decimal number like "0."
            if (waitingForRightOperand)
            {
                display.Text = "0.";
                waitingForRightOperand = false;
                return;
            }

            if (!display.Text.Contains("."))
                display.Text += ".";
        }

        // Removes the last character from the display.
        private void backBtn_Click(object sender, EventArgs e)
        {
            if (display.Text.Length > 1)
            {
                display.Text = display.Text.Substring(0, display.Text.Length - 1);
            }
            else
            {
                display.Text = "0";
            }
        }

        // Toggles the sign of the displayed value while safely handling invalid input.
        private void posNegBtn_Click(object sender, EventArgs e)
        {
            double? value = GetDisplayValueOrNull();
            if (value == null)
            {
                NotifyAndReset("Invalid number on display. Calculator has been cleared.");
                return;
            }

            display.Text = (-value.Value).ToString();
        }

        private void plusBtn_Click(object sender, EventArgs e) => mathOpBTN_Click(sender, e);
        private void minusBtn_Click(object sender, EventArgs e) => mathOpBTN_Click(sender, e);
        private void multBtn_Click(object sender, EventArgs e) => mathOpBTN_Click(sender, e);
        private void divBtn_Click(object sender, EventArgs e) => mathOpBTN_Click(sender, e);

        // Processes an operator selection and prepares for the next operand.
        private void mathOpBTN_Click(object sender, EventArgs e)
        {
            string op = ((Button)sender).Text.Trim();

            // If user presses another operator immediately (before typing the next number),
            // just update the operator and keep the existing leftOperand.
            if (!string.IsNullOrWhiteSpace(mathOperator) && waitingForRightOperand)
            {
                mathOperator = op;
                return;
            }

            double? value = GetDisplayValueOrNull();
            if (value == null)
            {
                NotifyAndReset("Invalid number on display. Calculator has been cleared.");
                return;
            }

            leftOperand = value.Value;
            mathOperator = op;

            display.Text = "0";
            waitingForRightOperand = true;
        }

        // Evaluates the selected arithmetic operation and updates the display with the result.
        private void equalsBtn_Click(object sender, EventArgs e)
        {
            double? value = GetDisplayValueOrNull();
            if (value == null)
            {
                NotifyAndReset("Invalid number on display. Calculator has been cleared.");
                return;
            }

            // -----------------------------
            // Repeated Equals Behavior
            // If '=' is pressed again with no current operator, repeat last operation.
            // Example: 5 + 2 = (7), then '=' again applies + 2 (9), again (11), etc.
            // -----------------------------
            if (string.IsNullOrWhiteSpace(mathOperator))
            {
                if (lastOperator == null || lastOperand == null)
                    return; // nothing to repeat

                double currentLeft = value.Value;
                double resultRepeat;

                switch (lastOperator)
                {
                    case "+":
                        resultRepeat = currentLeft + lastOperand.Value;
                        break;

                    case "-":
                        resultRepeat = currentLeft - lastOperand.Value;
                        break;

                    case "x":
                    case "X":
                    case "*":
                        resultRepeat = currentLeft * lastOperand.Value;
                        break;

                    case "/":
                        if (lastOperand.Value == 0)
                        {
                            NotifyAndReset("Cannot divide by zero.");
                            return;
                        }
                        resultRepeat = currentLeft / lastOperand.Value;
                        break;

                    default:
                        NotifyAndReset("Unknown operator. Calculator has been cleared.");
                        return;
                }

                display.Text = resultRepeat.ToString();
                leftOperand = resultRepeat;
                waitingForRightOperand = false;
                return;
            }

            // -----------------------------
            // Normal Equals Behavior
            // Compute leftOperand (stored when operator was pressed) with the current display as rightOperand.
            // Store lastOperator/lastOperand so '=' can repeat the same operation again.
            // -----------------------------
            double rightOperand = value.Value;

            // Store for repeated equals (modern calculator behavior).
            lastOperand = rightOperand;
            lastOperator = mathOperator;

            double result;

            switch (mathOperator)
            {
                case "+":
                    result = leftOperand + rightOperand;
                    break;

                case "-":
                    result = leftOperand - rightOperand;
                    break;

                case "x":
                case "X":
                case "*":
                    result = leftOperand * rightOperand;
                    break;

                case "/":
                    if (rightOperand == 0)
                    {
                        NotifyAndReset("Cannot divide by zero.");
                        return;
                    }
                    result = leftOperand / rightOperand;
                    break;

                default:
                    NotifyAndReset("Unknown operator. Calculator has been cleared.");
                    return;
            }

            display.Text = result.ToString();
            leftOperand = result;

            // Clear the pending operator so a second '=' will repeat using lastOperator/lastOperand.
            mathOperator = string.Empty;
            waitingForRightOperand = false;
        }
    }
}
