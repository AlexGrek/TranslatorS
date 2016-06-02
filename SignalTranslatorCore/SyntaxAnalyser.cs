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
                _errorMsg = "Unexpected end of file";
                Except = new SyntaxException("Unexpected end of file");
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

        public void Keyword(string keyword, TreeNode<SyntaxNode> tree)
        {
            if (v[index] == input.Keywords[keyword])
            {
                var node = CreateNode(keyword, v[index]);
                tree.AddNode(node);
                index++;
            }
            else Error(keyword);
        }

        public bool IsKeyword(string keyword)
        {
            if (v[index] == input.Keywords[keyword])
            {
                return true;
            }
            else return false;
        }

        public bool IsDelimiter(string del)
        {
            if (v[index] == input.Delimiters[del])
            {
                return true;
            }
            else return false;
        }

        public void Del(string delimiter, TreeNode<SyntaxNode> tree)
        {
            if (v[index] == input.Delimiters[delimiter])
            {
                var node = CreateNode(delimiter, v[index]);
                tree.AddNode(node);
                index++;
            }
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

        public bool IsConstant(int i)
        {
            var value = v[i];
            return value >= 400 && value < 500;
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
            if (IsKeyword("PROGRAM"))
            {
                var node = CreateNode("program");
                Keyword("PROGRAM", node);
                ProcedureIdentifier(node);
                Del(";", node);
                Block(node);
                Del(".", node);
                tree.AddNode(node);
            }
            else
            {
                // so let's try "PROCEDURE"
                var node = CreateNode("program");
                Keyword("PROCEDURE", node);
                ProcedureIdentifier(node);
                ParametersList(node);
                Del(";", node);
                Block(node);
                Del(";", node);
                tree.AddNode(node);
            }
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
            var node = CreateNode("identifier", Decode(v[index]), v[index]);
            index++;
            tree.AddNode(node);
        }

        private void Block(TreeNode<SyntaxNode> tree)
        {

            var node = CreateNode("block");
            Declarations(node);
            Keyword("BEGIN", node);
            StatementsList(node);
            Keyword("END", node);
            tree.AddNode(node);
        }

        private void Declarations(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("declarations");

            LabelDeclarations(node);
            VariableDeclarations(node);
            MathFunctionDeclarations(node);
            ProcedureDeclarations(node);
            ConstDeclarations(node);

            tree.AddNode(node);
        }

        private void StatementsList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("statements-list");
            bool isStLst = Statement(node);
            if (isStLst)
                StatementsList(node);
            else
                Empty(node);

            tree.AddNode(node);
        }

        private bool Statement(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("statement");
            if (IsConstant(index))
            {
                UnsignedInteger(node);
                Del(":", node);
                Statement(node);
                tree.AddNode(node);
                return true;
            }
            else
            if (IsIdentifier(index))
            {
                ProcedureIdentifier(node);
                ActualArguments(node);
                Del(";", node);
                tree.AddNode(node);
                return true;
            }
            else if (IsKeyword("LOOP"))
            {
                Keyword("LOOP", node);
                StatementsList(node);
                Keyword("ENDLOOP", node);
                Del(";", node);
                tree.AddNode(node);
                return true;
            }
            else if (IsKeyword("GOTO"))
            {
                Keyword("GOTO", node);
                UnsignedInteger(node);
                Del(";", node);
                tree.AddNode(node);
                return true;
            }
            else if (IsKeyword("LINK"))
            {
                Keyword("LINK", node);
                VariableIdentifier(node);
                UnsignedInteger(node);
                Del(";", node);
                tree.AddNode(node);
                return true;
            }
            else if (IsKeyword("IN"))
            {
                Keyword("IN", node);
                UnsignedInteger(node);
                Del(";", node);
                tree.AddNode(node);
                return true;
            }
            else if (IsKeyword("OUT"))
            {
                Keyword("OUT", node);
                UnsignedInteger(node);
                Del(";", node);
                tree.AddNode(node);
                return true;
            }
            else if (IsKeyword("RETURN"))
            {
                Keyword("RETURN", node);
                Del(";", node);
                tree.AddNode(node);
                return true;
            }
            else if (IsDelimiter(";"))
            {
                Del(";", node);
                tree.AddNode(node);
                return true;
            }
            else if (IsDelimiter("($"))
            {
                Del("($", node);
                AssemblyInsertFileIdentifier(node);
                Del("$)", node);
                tree.AddNode(node);
                return true;
            }
            else
                return false;
        }

        private void ActualArguments(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("actual-arguments");
            if (IsDelimiter("("))
            {
                Del("(", node);
                VariableIdentifier(node);
                IdentifiersList(node);
                Del(")", node);
            }
            else
                Empty(node);

            tree.AddNode(node);
        }

        private void LabelDeclarations(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("label-declarations");
            if (IsKeyword("LABEL"))
            {
                Keyword("LABEL", node);
                UnsignedInteger(node);
                LabelsList(node);
                Del(";", node);
            }
            else
                Empty(node);

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

            if (IsDelimiter(","))
            {
                Del(",", node);
                UnsignedInteger(node);
                LabelsList(node);
            }
            else
                Empty(node);

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

            if (IsKeyword("VAR"))
            {
                Keyword("VAR", node);
                DeclarationsList(node);
            }
            else
                Empty(node);

            tree.AddNode(node);
        }

        private void DeclarationsList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("declarations-list");

            if (IsIdentifier(index))
            {
                Declaration(node);
                DeclarationsList(node);
            }
            else
                Empty(node);

            tree.AddNode(node);
        }

        private void Declaration(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("declaration");
            VariableIdentifier(node);
            IdentifiersList(node);
            Del(":", node);
            Attribute_(node);
            Del(";", node);
            tree.AddNode(node);
        }

        private void Attribute_(TreeNode<SyntaxNode> tree)
        {
            if (IsAttribute(index))
            {
                var node = CreateNode("attribute", v[index]);
                index++;
                tree.AddNode(node);
            }
            else Error("INTEGER | FLOAT | BLOCKFLOAT");
        }

        private bool IsAttribute(int i)
        {
            return (v[i] ==
                input.Keywords["FLOAT"] ||
                v[i] ==
                input.Keywords["BLOCKFLOAT"] ||
                v[i] ==
                input.Keywords["INTEGER"]);
        }

        private void IdentifiersList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("identifiers-list");

            if (IsDelimiter(","))
            {
                Del(",", node);
                VariableIdentifier(node);
                IdentifiersList(node);
            }
            else
                Empty(node);

            tree.AddNode(node);
        }

        private void MathFunctionDeclarations(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("math-functon-declarations");

            if (IsKeyword("DEFFUNC"))
            {
                Keyword("DEFFUNC", node);
                FunctionList(node);
            }
            else
                Empty(node);

            tree.AddNode(node);
        }

        private void FunctionList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("function-list");

            tree.AddNode(node);

            if (IsIdentifier(index))
            {
                Function(node);
                FunctionList(node);
            }
            else
            {
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void Function(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("function");
            FunctionIdentifier(node);
            Del("=", node);
            Expression(node);
            FunctionCharacteristic(node);
            Del(";", node);
            tree.AddNode(node);
        }

        private void FunctionCharacteristic(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("function-characteristic");
            Del(@"\", node);
            UnsignedInteger(node);
            Del(",", node);
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
            if (IsKeyword("PROCEDURE"))
            {
                Procedure(node);
                ProcedureDeclarations(node);
            }
            else
            {
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void Procedure(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("procedure");
            Keyword("PROCEDURE", node);
            ProcedureIdentifier(node);
            ParametersList(node);
            Del(";", node);
            tree.AddNode(node);
        }

        private void ParametersList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("parameters-list");

            if (IsDelimiter("("))
            {
                Del("(", node);
                AttributesList(node);
                Del(")", node);
            }
            else
            {
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void AttributesList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("attributes-list");

            if (IsAttribute(index))
            {
                Attribute_(node);
                Del(",", node);
                AttributesList(node);
            }
            else
                Empty(node);

            tree.AddNode(node);
        }

        private void ConstList(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("parameters-list");

            if (IsDelimiter("("))
            {
                Del("(", node);
                AttributesList(node);
                Del(")", node);
            }
            else
            {
                Empty(node);
            }

            tree.AddNode(node);
        }

        private void ConstDeclarations(TreeNode<SyntaxNode> tree)
        {
            var node = CreateNode("const-declarations");

            if (IsKeyword("CONST"))
            {
                Keyword("CONST", node);
                VariableIdentifier(node);
                Del("=", node);
                UnsignedInteger(node);
                Del(";", node);
            }
            else
                Empty(node);

            tree.AddNode(node);
        }

        #endregion
    }
}
