using System;
using System.Text;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // Native LoadDefaults/LoadForcedSettings/LoadServiceSettings and CFGListBuilder<int>.
    // Parsed-data, iterator and parser ownership are genuine required caller bindings.
    public sealed class OriginalServiceSettingsLoader
    {
        public enum Kind{Defaults,Forced,Services}
        public sealed class State
        {
            public readonly OriginalSystemConfiguration.SharedState Configuration;
            private readonly List<byte[]> prerequisites=new List<byte[]>(),warnings=new List<byte[]>();
            public State(OriginalSystemConfiguration.SharedState configuration){Configuration=configuration??throw new ArgumentNullException(nameof(configuration));}
            public byte[][] Prerequisites{get{return Snapshot(prerequisites);}}
            public byte[][] IncompatibilityWarnings{get{return Snapshot(warnings);}}
            private static byte[][] Snapshot(List<byte[]> list){var a=new byte[list.Count][];for(int i=0;i<a.Length;++i)a[i]=(byte[])list[i].Clone();return a;}
            internal void Append(bool prerequisite,byte[] value){(prerequisite?prerequisites:warnings).Add((byte[])value.Clone());}
        }
        public sealed class Parser{public Func<Node> GetParsedData;public Action Delete;}
        public sealed class Node
        {
            public Func<byte[],bool> HasSubtree;
            public Func<byte[],Node> GetSubtree;
            public Func<byte[],Iterator> SelectByName;
            public Func<byte[],byte[]> Attribute;
            public Action Delete;
        }
        public sealed class Iterator{public Func<bool> HasNext;public Func<Node> Next;public Action Delete;}
        public sealed class Host{public Func<byte[],bool> Readable;public Func<byte[],Parser> CreateHybridParser;public Action RemoveSingleton;}
        private readonly State state;private readonly Host host;
        public OriginalServiceSettingsLoader(State state,Host bindings)
        {
            this.state=state??throw new ArgumentNullException(nameof(state));
            if(bindings==null||bindings.Readable==null||bindings.CreateHybridParser==null||bindings.RemoveSingleton==null)throw new ArgumentException("Actual access/Hybrid factory owner required");
            host=new Host{Readable=bindings.Readable,CreateHybridParser=bindings.CreateHybridParser,RemoveSingleton=bindings.RemoveSingleton};
        }
        private static byte[] U(string s){return Encoding.UTF8.GetBytes(s);}
        private static bool Equal(byte[] a,string s){byte[] b=U(s);if(a.Length!=b.Length)return false;for(int i=0;i<b.Length;++i)if(a[i]!=b[i])return false;return true;}
        public void ParseServiceSetting(byte[] suppliedName,byte[] suppliedValue)
        {
            if(suppliedName==null||suppliedValue==null)throw new ArgumentNullException();byte[] name=(byte[])suppliedName.Clone(),value=(byte[])suppliedValue.Clone();
            if(Equal(name,"FAULT"))throw new OriginalSystemConfigurationFault("Service settings FAULT",value);
            if(Equal(name,"Satisfy Prerequisite")){state.Append(true,value);return;}
            if(Equal(name,"Set Incompatability")){state.Append(false,value);return;}
            var owner=state.Configuration.GlobalOwner;if(owner==null)throw new InvalidOperationException("Native global SystemConfiguration missing");owner.AddConfigurable(name,value);
        }
        private static void CheckIterator(Iterator i)
        {if(i==null||i.HasNext==null||i.Next==null||i.Delete==null)throw new ArgumentException("Actual parsed iterator required");}
        private void AddStructsFrom(Node subtree)
        {
            if(subtree.SelectByName==null)throw new ArgumentException("Actual parsed selector required");
            Iterator structs=subtree.SelectByName(U("struct"));CheckIterator(structs);
            while(structs.HasNext())
            {
                Node s=structs.Next();if(s==null||s.SelectByName==null)throw new ArgumentException("Actual struct node required");
                Iterator fields=s.SelectByName(U("field"));CheckIterator(fields);
                while(fields.HasNext())
                {
                    Node f=fields.Next();if(f==null||f.Attribute==null)throw new ArgumentException("Actual field node required");
                    byte[] name=f.Attribute(U("name"));byte[] value=f.Attribute(U("value"));ParseServiceSetting(name,value);
                }
                fields.Delete();
            }
            structs.Delete();
        }
        public void Load(Kind kind,byte[] filename)
        {
            if(filename==null)throw new ArgumentNullException(nameof(filename));if(kind<Kind.Defaults||kind>Kind.Services)throw new ArgumentOutOfRangeException(nameof(kind));
            int end=Array.IndexOf(filename,(byte)0);if(end<0)end=filename.Length;var path=new byte[end];Array.Copy(filename,path,end);
            if(!host.Readable(path))return;
            Parser parser=host.CreateHybridParser(path);if(parser==null||parser.GetParsedData==null||parser.Delete==null)throw new ArgumentException("Actual Hybrid parser required");
            Node root=parser.GetParsedData();if(root==null||root.HasSubtree==null||root.GetSubtree==null||root.Delete==null)throw new ArgumentException("Actual parsed root required");
            byte[] selector=U(kind==Kind.Defaults?"system_default_settings":kind==Kind.Forced?"ServiceDefinitions.system_forced_settings":"Services");
            if(!root.HasSubtree(selector))throw new OriginalSystemConfigurationFault("Required settings subtree missing",selector);
            Node subtree=root.GetSubtree(selector);if(subtree==null)throw new OriginalSystemConfigurationFault("Settings subtree returned null",selector);
            if(subtree.Delete==null)throw new ArgumentException("Actual subtree deletion required");
            if(kind!=Kind.Forced)state.Configuration.DefaultsLoaded=0;
            AddStructsFrom(subtree);
            if(kind!=Kind.Forced)state.Configuration.DefaultsLoaded=1;
            subtree.Delete();root.Delete();parser.Delete();host.RemoveSingleton();
            // Native faults/throws do not execute the success-path Deletes/reset.
        }
    }
}
