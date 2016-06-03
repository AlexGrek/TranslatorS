using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTranslatorCore
{
    public class CodeGenerator
    {
        LexAn _lex;
        Tree<SyntaxNode> _tree;
        public StringBuilder Output;
        public List<int> Labels = new List<int>();
        public List<Variable> Vars = new List<Variable>();
        public List<Procedure> Procedures = new List<Procedure>();
        int _uniquename = 1;

        public CodeGenerator(LexAn lex, Tree<SyntaxNode> tree)
        {
            _lex = lex;
            _tree = tree;
            Output = new StringBuilder();
        }

        private void AddLabel(TreeNode<SyntaxNode> unsignedint)
        {
            var code = unsignedint.Content.Value;
            var ints = _lex.Constants.FindKey(code);
            int i = int.Parse(ints);
            Labels.Add(i);
        }

        public string GenerateCode()
        {
            var sigprogram = _tree.Root[0];
            Block(sigprogram[3]);
            
            //TEST
            foreach (var i in Labels)
            {
                Output.AppendFormat("{0}, ", i);
            }
            
            return Output.ToString();
        }

        void Block(TreeNode<SyntaxNode> block)
        {
            Declarations(block[0]);
            Output.AppendLine("CODE SEGMENT");
            StatList(block[2]);
            Output.AppendLine("CODE ENDS");
        }

        void StatList(TreeNode<SyntaxNode> stlist)
        {
            if (stlist.Content.Name != "statements-list")
            {
                if (stlist.Content.Name == "empty")
                    return;
                else
                    throw new ArgumentException(stlist.Content.Name + " is not st-list");
            }

            Statement(stlist[0]);
            if (stlist[1][0].Content.Name != "empty")
            {
                StatList(stlist[1]);
            }
        }

        void Statement(TreeNode<SyntaxNode> st)
        {
            //GOTO
            if (st[0].Content.Value == 311)
            {
                var where = _lex.Constants.FindKey(st[1].Content.Value);
                if (!Labels.Contains(int.Parse(where)))
                    throw new SemanticException($"Label {where} is not defined.");
                Output.AppendLine($"GOTO @{where}");
            }
            //LOOP
            else if (st[0].Content.Value == 309)
            {
                var label = "@loop" + _uniquename++;
                Output.AppendLine($"{label}:");
                StatList(st[1]);
                Output.AppendLine($"JMP  {label}");
            }
            //;
            else if (st[0].Content.Value == 3)
            {
                return;
            }
            //OUT
            else if (st[0].Content.Value == 314)
            {
                var port = _lex.Constants.FindKey(st[1].Content.Value);
                Output.AppendLine($"OUT {port}, AL");
            }
            //IN
            else if (st[0].Content.Value == 313)
            {
                var port = _lex.Constants.FindKey(st[1].Content.Value);
                Output.AppendLine($"IN AL, {port}");
            }
            //: (label)
            else if (st[1].Content.Value == 6)
            {
                var port = _lex.Constants.FindKey(st[0].Content.Value);
                Output.AppendLine($"@{port}:");
                Statement(st[2]);
            }
            //return
            else if (st[0].Content.Value == 315)
            {
                Output.AppendLine("RET");
            }
            //procedure call
            else if (st[0].Content.Name == "procedure-identifier")
            {
                DealWithProcedureCall(st);
            }
        }

        void DealWithProcedureCall(TreeNode<SyntaxNode> st)
        {
            var name = _lex.Identifiers.FindKey(st[0][0].Content.Value);

            Procedure? proc = null;

            foreach (var item in Procedures)
            {
                if (item.Name == name)
                    proc = item;
            }

            if (proc.HasValue == false)
                throw new SemanticException("No such procedure declared: " + name);

            if (st[1][0].Content.Name != "empty")
            {
                var acctargs = st[1];
                CheckArg(acctargs[1], 0, proc.Value); //check first argument
                Argument(acctargs[2], 1, proc.Value); //and the others
               
            }

            Output.AppendLine($"CALL {name}");

        }

        void Argument(TreeNode<SyntaxNode> idlist, int index, Procedure p)
        {
            if (idlist.Content.Name == "empty")
            {
                //check args count
                if (p.Params.Count != index )
                    throw new SemanticException($"Arguments count mismatch: expected {p.Params.Count}, got {index}");
                return;
            }
            if (idlist.Nodes.Count < 2)
            {
                Argument(idlist[0], index, p);
                return;
            }
            CheckArg(idlist[1], index, p);
            Argument(idlist[2], ++index, p);
        }

        void CheckArg(TreeNode<SyntaxNode> varid, int index, Procedure p)
        {
            var arg = _lex.Identifiers.FindKey(varid[0].Content.Value);

            Variable? a = null;

            foreach (var item in Vars)
            {
                if (item.Name == arg)
                    a = item;
            }
            if (a.HasValue == false)
                throw new SemanticException("No such var declared: " + arg);

            if (p.Params.Count <= index)
                throw new SemanticException($"Args count mismatch: expected {p.Params.Count}, got {index + 1}");

            if (a.Value.VariableType != p.Params[index])
                throw new SemanticException($"Type mismatch: expected {p.Params[index]}, got {a.Value.VariableType}");

            Output.AppendLine("PUSH  " + arg);
        }

        void Declarations(TreeNode<SyntaxNode> decl)
        {
            LabelDecl(decl[0]);
            VariableDecl(decl[1]);
            ProcedureDeclarations(decl[3]);
        }

        void LabelDecl(TreeNode<SyntaxNode> labeldecl)
        {
            AddLabel(labeldecl[1]);
            LabelList(labeldecl[2]);
        }

        void LabelList(TreeNode<SyntaxNode> labell)
        {
            AddLabel(labell[1]);
            if (labell[2][0].Content.Name != "empty")
            {
                LabelList(labell[2]);
            }
        }

        void VariableDecl(TreeNode<SyntaxNode> vdecls)
        {
            Output.AppendLine("DATA SEGMENT");
            DeclarList(vdecls[1]);
            Output.AppendLine("DATA ENDS");
        }

        void DeclarList(TreeNode<SyntaxNode> declllist)
        {
            Declaration(declllist[0]);
            if (declllist[1][0].Content.Name != "empty")
            {
                DeclarList(declllist[1]);
            }
        }

        void Declaration(TreeNode<SyntaxNode> decl)
        {
            var thetype = _lex.Keywords.FindKey(decl[3].Content.Value);
            var thename = _lex.Identifiers.FindKey(decl[0][0].Content.Value);

            var newvar = new Variable()
            {
                VariableType = (VarType)Enum.Parse(typeof(VarType), thetype),
                Name = thename
            };

            if (Vars.Contains(newvar))
                throw new SemanticException($"Variable '{thename}' declared more than once.");
            Vars.Add(newvar);
            Output.AppendLine(newvar.Generate());
        }

        void ProcedureDeclarations(TreeNode<SyntaxNode> decl)
        {
            ProcedureHandler(decl[0]);

            if (decl[1][0].Content.Name != "empty")
            {
                ProcedureDeclarations(decl[1]);
            }
        }

        void ProcedureHandler(TreeNode<SyntaxNode> decl)
        {
            var thename = _lex.Identifiers.FindKey(decl[1][0].Content.Value);

            var newvar = new Procedure()
            {
                Name = thename
            };

            if (decl[2][0].Content.Name != "empty")
                newvar.AddParameters(decl[2][1], _lex);

            if (Procedures.Contains(newvar))
                throw new SemanticException($"Procedure '{thename}' declared more than once.");
            Procedures.Add(newvar);
        }

        public enum VarType { FLOAT, BLOCKFLOAT, INTEGER }

        public struct Variable
        {
            public VarType VariableType;
            public string Name;

            public string Generate()
            {
                var typeword = "";
                switch (VariableType)
                {
                    case VarType.BLOCKFLOAT:
                        typeword = "DQ";
                        break;
                    case VarType.FLOAT:
                        typeword = "DDQ";
                        break;
                    case VarType.INTEGER:
                        typeword = "DD";
                        break;
                }

                return $"{Name}   {typeword}   ?";
            }

            public override bool Equals(object obj)
            {
                return (obj is Variable) && (((Variable)obj).Name.Equals(this.Name));
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }

        public struct Procedure
        {
            public List<VarType> Params;
            public string Name;

            public void AddParameters(TreeNode<SyntaxNode> decl, LexAn lex)
            {
                if (decl.Content.Name != "attributes-list")
                    throw new ArgumentException(decl.Content.Name + " is not attributes-list");

                Params = Params != null ? Params : new List<VarType>();

                var thetype = lex.Keywords.FindKey(decl[0].Content.Value);
                Params.Add((VarType)Enum.Parse(typeof(VarType), thetype));
                if (decl[2][0].Content.Name != "empty")
                {
                    AddParameters(decl[2], lex);
                }
            }

            public override bool Equals(object obj)
            {
                return (obj is Variable) && (((Variable)obj).Name.Equals(this.Name));
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }

    }

}
