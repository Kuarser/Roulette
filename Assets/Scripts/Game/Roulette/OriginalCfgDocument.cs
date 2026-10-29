using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    public sealed class OriginalCfgPathException:Exception
    {public OriginalCfgPathException(string message):base(message){}}
    // Managed DOM transport for recovered CFGParsedData normal tree operations.
    // Caller supplies the native narrow-string encoding; no locale is invented.
    public sealed class OriginalCfgDocument:IDisposable
    {
        private readonly XmlDocument document;private readonly Encoding narrow;private bool deleted;
        private OriginalCfgDocument(XmlDocument document,Encoding encoding){this.document=document;narrow=encoding;}
        public static OriginalCfgDocument FromBytes(byte[] bytes,Encoding encoding)
        {
            if(bytes==null||encoding==null)throw new ArgumentNullException();
            var doc=new XmlDocument{PreserveWhitespace=true,XmlResolver=null};
            using(var stream=new MemoryStream(bytes,false))doc.Load(stream);
            return new OriginalCfgDocument(doc,encoding);
        }
        public static OriginalCfgDocument OpenFile(string path,Encoding encoding){return FromBytes(File.ReadAllBytes(path),encoding);}
        private void Check(){if(deleted)throw new ObjectDisposedException(nameof(OriginalCfgDocument));}
        public Node GetParsedData(){Check();return new Node(this,document.DocumentElement);}
        public void Dispose(){deleted=true;}
        private string CString(byte[] raw){if(raw==null)throw new ArgumentNullException();int n=Array.IndexOf(raw,(byte)0);return narrow.GetString(raw,0,n<0?raw.Length:n);}
        private bool NameEquals(string text,byte[] bytes){byte[] name=narrow.GetBytes(text);if(name.Length!=bytes.Length)return false;for(int i=0;i<name.Length;++i)if(name[i]!=bytes[i])return false;return true;}
        public sealed class Node
        {
            private readonly OriginalCfgDocument owner;private XmlElement element;private bool deleted;
            internal Node(OriginalCfgDocument owner,XmlElement element){this.owner=owner;this.element=element;}
            private void Check(){owner.Check();if(deleted)throw new ObjectDisposedException("CFGParsedData");}
            internal void SetRoot(XmlElement root){Check();element=root;}
            public void Delete(){Check();deleted=true;}
            public byte[] Attribute(byte[] name){Check();return owner.narrow.GetBytes(element==null?"":element.GetAttribute(owner.CString(name)));}
            public byte[] TagName(){Check();return owner.narrow.GetBytes(element==null?"":element.Name);}
            public bool HasAttributes(){Check();return element!=null&&element.HasAttributes;}
            public Iterator GetChildren(){Check();return new Iterator(owner,element==null?null:element.ChildNodes);}
            public bool HasChildren(){Check();var iterator=GetChildren();bool result=iterator.HasNext();iterator.Delete();return result;}
            public byte[] Text()
            {
                Check();XmlNode first=element==null?null:element.FirstChild;if(first==null)return new byte[0];
                // Xerces reports ordinary/ignorable whitespace as TEXT_NODE too.
                if(first.NodeType!=XmlNodeType.Text&&first.NodeType!=XmlNodeType.Whitespace&&first.NodeType!=XmlNodeType.SignificantWhitespace)
                    throw new OriginalCfgPathException("Expected TEXT_NODE");
                return owner.narrow.GetBytes(first.Value??"");
            }
            public Iterator SelectByName(byte[] name)
            {Check();return new Iterator(owner,element==null?null:element.GetElementsByTagName(owner.CString(name)));}
            public Node GetSubtree(byte[] suppliedPath)
            {
                Check();if(suppliedPath==null)throw new ArgumentNullException();
                // Native strtok_r with '.' delimiter skips empty tokens and stops at NUL.
                string path=owner.CString(suppliedPath);string[] tokens=path.Split(new[]{'.'},StringSplitOptions.RemoveEmptyEntries);
                if(tokens.Length==0)throw new OriginalCfgPathException("Empty subtree path");
                int first=element!=null&&owner.NameEquals(element.Name,owner.narrow.GetBytes(tokens[0]))?1:0;XmlElement current=element;
                for(int i=first;i<tokens.Length;++i)
                {
                    XmlElement selected=null;int count=0;byte[] name=owner.narrow.GetBytes(tokens[i]);
                    if(current!=null)foreach(XmlNode child in current.ChildNodes)
                    {var e=child as XmlElement;if(e!=null&&owner.NameEquals(e.Name,name)){selected=e;++count;}}
                    if(count!=1)throw new OriginalCfgPathException(count==0?"No matching direct child":"Multiple matching direct children");
                    current=selected;
                }
                return new Node(owner,current);
            }
            public bool HasSubtree(byte[] path)
            {Check();try{Node n=GetSubtree(path);n.Delete();return true;}catch(OriginalCfgPathException){return false;}}
            public OriginalServiceSettingsLoader.Node Binding()
            {Check();return new OriginalServiceSettingsLoader.Node{HasSubtree=HasSubtree,GetSubtree=p=>GetSubtree(p).Binding(),SelectByName=p=>SelectByName(p).Binding(),Attribute=Attribute,Delete=Delete};}
        }
        public sealed class Iterator
        {
            private readonly OriginalCfgDocument owner;private readonly XmlNodeList list;private readonly int length;
            private readonly Node reusable;private int cursor;private bool deleted;
            internal Iterator(OriginalCfgDocument owner,XmlNodeList list){this.owner=owner;this.list=list;length=list==null?0:list.Count;reusable=new Node(owner,null);}
            private void Check(){owner.Check();if(deleted)throw new ObjectDisposedException("CFGParsedDataIterator");}
            public bool HasNext(){Check();for(int i=cursor;i<length;++i)if(list[i].NodeType==XmlNodeType.Element)return true;return false;}
            public Node Next()
            {
                Check();if(!HasNext())return null;
                XmlElement e;do{e=list[cursor++] as XmlElement;}while(e==null);reusable.SetRoot(e);return reusable;
            }
            public void Delete(){Check();reusable.Delete();deleted=true;}
            public OriginalServiceSettingsLoader.Iterator Binding()
            {Check();OriginalServiceSettingsLoader.Node binding=reusable.Binding();return new OriginalServiceSettingsLoader.Iterator{HasNext=HasNext,Next=()=>Next()==null?null:binding,Delete=Delete};}
        }
        public static OriginalServiceSettingsLoader.Host BindServiceLoader(Func<byte[],string> resolvePath,Encoding encoding)
        {
            if(resolvePath==null||encoding==null)throw new ArgumentNullException();
            return new OriginalServiceSettingsLoader.Host{
                Readable=raw=>{string path=resolvePath(raw);try{using(var file=File.OpenRead(path))return true;}catch(IOException){return false;}catch(UnauthorizedAccessException){return false;}},
                CreateHybridParser=raw=>{var doc=OpenFile(resolvePath(raw),encoding);return new OriginalServiceSettingsLoader.Parser{GetParsedData=()=>doc.GetParsedData().Binding(),Delete=doc.Dispose};},
                RemoveSingleton=()=>{} // Native093ffc10 is REP RET (f3c3), no state mutation.
            };
        }
    }
}
