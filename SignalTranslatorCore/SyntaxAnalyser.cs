using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SignalTranslatorCore
{
    public class SyntaxAnalyser
    {
        internal LexAn input;
        private int _index;
        internal int index
        {
            get { return _index; }
            set
            {
                _index = value;
                if (LexIndexChanged != null)
                    LexIndexChanged(value);
            }
        }
        internal int[] v;
        public Tree<SyntaxNode> Tree {get; internal set;}
        public bool IsValid = false;
        internal string _errorMsg;
        public string ErrorMessage {
            get { return _errorMsg; }
            internal set { _errorMsg = value; ErrorStackTrace.Push(value); } }
        public Stack<string> ErrorStackTrace { get; internal set; } = new Stack<string>();
        public SyntaxException Except { get; private set; }
        public event Action<int> LexIndexChanged;

        public SyntaxAnalyser(LexAn lex)
        {
            input = lex;
            v = lex.Output.ToArray();
        }

        public void BuildAST()
        {
            Tree = new Tree<SyntaxNode>(new SyntaxNode("treeee"));
            try {
                Program(Tree.Root);
                IsValid = true;
            } catch (IndexOutOfRangeException)
            {
                IsValid = false;
            }
            catch(SyntaxException ex)
            {
                Except = ex;
            }
        }

        public TreeNode<SyntaxNode> CreateNode(string name, int value = -1)
        {
            return new TreeNode<SyntaxNode>(new SyntaxNode(name, value));
        }

        public TreeNode<SyntaxNode> CreateNode(string name, string tag, int value = -1)
        {
            return new TreeNode<SyntaxNode>(new SyntaxNode(name, value, tag));
        }

        public void Keyword(string keyword)
        {
            if (v[index] == input.Keywords[keyword])
                index++;
            else Error(keyword);
        }

        public void Del(string delimiter)
        {
            if (v[index] == input.Delimiters[delimiter])
                index++;
            else Error(delimiter);
        }

        public string Decode(int i)
        {
            if (i >= 500)
            {
                //search in identifiers
                foreach(var pair in input.Identifiers)
                {
                    if (pair.Value == i)
                        return pair.Key;
                }
                return null;
            }

            if (i > 399)
            {
                //search in constants
                foreach (var pair in input.Constants)
                {
                    if (pair.Value == i)
                        return pair.Key;
                }
                return null;
            }

            if (i > 299)
            {
                //search in keywords
                foreach (var pair in input.Keywords)
                {
                    if (pair.Value == i)
                        return pair.Key;
                }
                return null;
            }

            if (i >= 0)
            {
                //search in delimiters
                foreach (var pair in input.Delimiters)
                {
                    if (pair.Value == i)
                        return pair.Key;
                }
                return null;
            }

            return null;
        }

        public bool IsIdentifier(int i)
        {
            var value = v[i];
            return value >= 500;
        }

        #region ErrorHandling
        public void Error(string expected, string found, bool critical = false, [CallerMemberName] string callerName = "")
        {
            var stackTrace = new StackTrace();
            var method = stackTrace.GetFrame(1).GetMethod().Name;
            if (method == "Error")
                method = stackTrace.GetFrame(2).GetMethod().Name;
            var line = input.LineOf(index);
            ErrorMessage = $"Error on line {line} (statement {index}): {expected} expected, but {found} found. \nIn method {method}.";
            throw new SyntaxException(expected, found, line, index, method, critical);
        }

        public void Error(string expected, bool critical = false)
        {
            var found = Decode(v[index]);
            if (found == null)
                Error(expected, "<UNKNOWN LEXEME>", true);
            Error(expected, found, critical);
        }

        public void Error()
        {
            var stackTrace = new StackTrace();
            var method = stackTrace.GetFrame(1).GetMethod().Name;
            var line = input.LineOf(index);
            ErrorMessage = $"Error on line {line} (statement {index}): in method {method}.";
            throw new SyntaxException(ErrorMessage);
        }
        #endregion

        #region TreeRecursiveBuilders

        private void Program(TreeNode<SyntaxNode> tree)
        {
            var i = index;
            try
            {
                Keyword("PROGRAM");
                var node = CreateNode("program");
                ProcedureIdentifier(node);
                Del(";");
                Block(node);
                Del(".");
                tree.AddNode(node);
            } catch (SyntaxException e)   //"PROGRAM" failed
            {
                if (e.Expected != "PROGRAM")
                    throw;
                // so let's try "PROCEDURE"
                index = i;
                Keyword("PROCEDURE");
                var node = CreateNode("program");
                ProcedureIdentifier(node);
                ParametersList(node);
                Del(";");
                Block(node);
                Del(";");
                tree.AddNode(node);
            }
            /*
            if (v[index] != Keyword("PROGRAM"))
            {
                //PROCEDURE < procedure - identifier > < parameters - list > ; < block > ;
                if (v[index] != Keyword("PROCEDURE"))
                    return Error("PROGRAM or PROCEDURE");
                index++;
                var proc = tree.AddNode(new SyntaxNode("program:procedure"));
                if (!ProcedureIdentifier(proc))
                    return false;
                index++;
                if (!ParametersList(proc))
                    return false;
                index++;
                if (v[index] != Del(";"))
                    return Error(";");
                index++;
                if (!Block(proc))
                    return false;
                index++;
                if (v[index] != Del(";"))
                    return Error(";");

                return true;
            }

            //PROGRAM <procedure-identifier> ;
            //< block >.
            index++;
            var next = tree.AddNode(new SyntaxNode("program"));
            if (!ProcedureIdentifier(next))
                return false;
            index++;
            if (v[index] != Del(";"))
                return Error(";");
            index++;
            if (!Block(next))
                return false;
            index++;

            if (v[index] != Del("."))
                return Error(".");

            return true;

    */
        }

        private void ProcedureIdentifier(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("procedure-identifier");
            Identifier(node);
            tree.AddNode(node);
        }

        private void Identifier(TreeNode<SyntaxNode> tree)
        {
            if (!IsIdentifier(index))
                Error("identifier");
            var node = CreateNode("identifier", v[index]);
            index++;
            tree.AddNode(node);
        }

        private void Block(TreeNode<SyntaxNode> tree)
        {

            var node = CreateNode("block");
            Declarations(node);
            Keyword("BEGIN");
            StatementsList(node);
            Keyword("END");
            tree.AddNode(node);
        }

        private void Declarations(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("declarations");

            LabelDeclarations(node);
            VariableDeclarations(node);
            MathFunctionDeclarations(node);
            ProcedureDeclarations(node);

            tree.AddNode(node);
        }

        private void StatementsList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("statements-list");
            int i = index;
            try
            {
                Statement(node);
                StatementsList(node);
            }
            catch (SyntaxException e)
            {
                if (e.Critical)
                    throw;
                index = i;
                node = CreateNode("statements-list");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void Statement(TreeNode<SyntaxNode> tree)
        {
            var i = index;
            try
            {
                var node = CreateNode("statement", ":");
                UnsignedInteger(node);
                Del(":");
                Statement(node);
                tree.AddNode(node);
                return;

            } catch (SyntaxException e)
            {
                if (e.Expected != "unsigned-integer")
                {
                    e.Critical = true;
                    throw;
                }
                index = i;
            }
            i = index;
            try
            {
                var node = CreateNode("statement", "procedure");
                ProcedureIdentifier(node);
                ActualArguments(node);
                Del(";");
                tree.AddNode(node);
                return;
            }
            catch (SyntaxException e)
            {
                if (e.Critical)
                    throw;
                index = i;
            }
            i = index;
            try
            {
                var node = CreateNode("statement", "loop");
                Keyword("LOOP");
                StatementsList(node);
                Keyword("ENDLOOP");
                Del(";");
                tree.AddNode(node);
                return;
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "LOOP")
                {
                    e.Critical = true;
                    throw;
                }
                index = i;
            }
            i = index;
            try
            {
                var node = CreateNode("statement", "goto");
                Keyword("GOTO");
                UnsignedInteger(node);
                Del(";");
                tree.AddNode(node);
                return;
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "GOTO")
                    throw;
                index = i;
            }
            i = index;
            try
            {
                var node = CreateNode("statement", "link");
                Keyword("LINK");
                VariableIdentifier(node);
                UnsignedInteger(node);
                Del(";");
                tree.AddNode(node);
                return;
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "LINK")
                    throw;
                index = i;
            }
            i = index;
            try
            {
                var node = CreateNode("statement", "in");
                Keyword("IN");
                UnsignedInteger(node);
                Del(";");
                tree.AddNode(node);
                return;
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "IN")
                    throw;
                index = i;
            }
            i = index;
            try
            {
                var node = CreateNode("statement", "out");
                Keyword("OUT");
                UnsignedInteger(node);
                Del(";");
                tree.AddNode(node);
                return;
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "OUT")
                    throw;
                index = i;
            }
            i = index;
            try
            {
                var node = CreateNode("statement", "return");
                Keyword("RETURN");
                Del(";");
                tree.AddNode(node);
                return;
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "RETURN")
                    throw;
                index = i;
            }
            i = index;
            try
            {
                var node = CreateNode("statement", ";");
                Del(";");
                tree.AddNode(node);
                return;
            }
            catch (SyntaxException)
            {
                index = i;
            }
            i = index;
            try
            {
                var node = CreateNode("statement", "assembly file");
                Del("($");
                AssemblyInsertFileIdentifier(node);
                Del("$)");
                tree.AddNode(node);
                return;
            }
            catch (SyntaxException e)
            {
                index = i;
                throw;
            }
        }

        private void ActualArguments(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("actual-arguments");
            int i = index;
            try
            {
                Del("(");
                VariableIdentifier(node);
                IdentifiersList(node);
                Del(")");
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "(")
                {
                    e.Critical = true;
                    throw;
                }
                index = i;
                node = CreateNode("actual-arguments");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void LabelDeclarations(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("label-declarations");
            int i = index;
            try
            {
                Keyword("LABEL");
                UnsignedInteger(node);
                LabelsList(node);
                Del(";");
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "LABEL")
                    throw;
                index = i;
                node = CreateNode("label-declarations");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void UnsignedInteger(TreeNode<SyntaxNode> tree)
        {  
            if (v[index] >= 400 && v[index] <= 500)
            {
                var node = CreateNode("unsigned-integer", v[index]);
                index++;
                tree.AddNode(node);
                return;
            }
            Error("unsigned-integer");   
        }

        private void LabelsList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("labels-list");

            int i = index;
            try
            {
                Del(",");
                UnsignedInteger(node);
                LabelsList(node);
            }
            catch (SyntaxException e)
            {
                if (e.Expected != ",")
                    throw;
                index = i;
                node = CreateNode("label-declarations");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void Empty(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("empty");
            tree.AddNode(node);
        }

        private void VariableDeclarations(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("variable-declarations");

            int i = index;
            try
            {
                Keyword("VAR");
                DeclarationsList(node);
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "VAR")
                    throw;
                index = i;
                node = CreateNode("variable-declarations");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void DeclarationsList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("declarations-list");

            int i = index;
            try
            {
                Declaration(node);
                DeclarationsList(node);
            }
            catch (SyntaxException)
            {
                index = i;
                node = CreateNode("declarations-list");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void Declaration(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("declaration");
            VariableIdentifier(node);
            IdentifiersList(node);
            Del(":");
            Attribute_(node);
            Del(";");
            tree.AddNode(node);
        }

        private void Attribute_(TreeNode<SyntaxNode> tree)
        {
            if (v[index] ==
                input.Keywords["FLOAT"] ||
                v[index] ==
                input.Keywords["BLOCKFLOAT"] ||
                v[index] ==
                input.Keywords["INTEGER"])
            {
                var node = CreateNode("attribute", v[index]);
                index++;
                tree.AddNode(node);
            }
            else Error("INTEGER | FLOAT | BLOCKFLOAT");
        }

        private void IdentifiersList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("identifiers-list");

            int i = index;
            try
            {
                Del(",");
                VariableIdentifier(node);
                IdentifiersList(node);
            }
            catch (SyntaxException)
            {
                index = i;
                node = CreateNode("identifiers-list");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void MathFunctionDeclarations(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("math-functon-declarations");

            int i = index;
            try
            {
                Keyword("DEFFUNC");
                FunctionList(node);
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "DEFFUNC")
                    throw;
                index = i;
                node = CreateNode("math-functon-declarations");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void FunctionList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("function-list");

            tree.AddNode(node);

            int i = index;
            try
            {
                Function(node);
                FunctionList(node);
            }
            catch (SyntaxException)
            {
                index = i;
                node = CreateNode("function-list");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void Function(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("function");
            FunctionIdentifier(node);
            Del("=");
            Expression(node);
            FunctionCharacteristic(node);
            Del(";");
            tree.AddNode(node);
        }

        private void FunctionCharacteristic(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("function-characteristic");
            Del(@"\");
            UnsignedInteger(node);
            Del(",");
            UnsignedInteger(node);
            tree.AddNode(node);
        }

        //WAS MISSING IN DOCUMENTATION!!!
        private void Expression(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("expression");
            Identifier(node);
            tree.AddNode(node);
        }

        private void FunctionIdentifier(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("function-identifier");
            Identifier(node);
            tree.AddNode(node);
        }

        private void VariableIdentifier(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("variable-identifier");
            Identifier(node);
            tree.AddNode(node);
        }

        private void AssemblyInsertFileIdentifier(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("assembly-insert-file-identifier");
            Identifier(node);
            tree.AddNode(node);
        }

        private void ProcedureDeclarations(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("procedure-declarations");
            int i = index;
            try
            {
                Procedure(node);
                ProcedureDeclarations(node);
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "PROCEDURE")
                    throw;
                index = i;
                node = CreateNode("procedure-declarations");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void Procedure(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("procedure");
            Keyword("PROCEDURE");
            ProcedureIdentifier(node);
            ParametersList(node);
            Del(";");
            tree.AddNode(node);
        }

        private void ParametersList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("parameters-list");

            int i = index;
            try
            {
                Del("(");
                AttributesList(node);
                Del(")");
            }
            catch (SyntaxException e)
            {
                if (e.Expected != "(")
                    throw;
                index = i;
                node = CreateNode("parameters-list");
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void AttributesList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("attributes-list");

            int i = index;
            try
            {
                Attribute_(node);
                Del(",");
                AttributesList(node);
            }
            catch (SyntaxException)
            {
                index = i;
                node = CreateNode("attributes-list");
                Empty(node);
            }

            tree.AddNode(node);
        }


        #endregion
    }
}
