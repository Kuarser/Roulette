using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace RouletteDeluxe
{
    // The supplied RCG graph uses static positions, bitmap/movie/audio/meter
    // atoms and triggers. Unsupported original object types fail explicitly.
    public sealed class OriginalFeatureGraph
    {
        public sealed class Atom
        {
            public string Type,Name;
            public readonly Dictionary<string,string> Properties=new Dictionary<string,string>();
            public int Integer(string key,int fallback=0)
            {
                string value;return Properties.TryGetValue(key,out value)?int.Parse(value,CultureInfo.InvariantCulture):fallback;
            }
        }
        public sealed class Trigger
        {
            public string Name,KillName;
            public int Index,WinDataOffset;
            public bool WaitForCompletion;
        }
        public sealed class Feature
        {
            public string Name;
            public bool Sequential;
            public short StartX,StartY,EndX,EndY;
            public int TransitionMilliseconds;
            public readonly List<Atom> Objects=new List<Atom>();
            public readonly List<Trigger> Triggers=new List<Trigger>();
        }
        private readonly List<Feature> features=new List<Feature>();
        private readonly Dictionary<string,int> ids=new Dictionary<string,int>(StringComparer.Ordinal);
        public int Count { get {return features.Count;} }
        public Feature this[int id] { get {return features[id];} }
        public int Id(string name)
        {
            int result;if(!ids.TryGetValue(name,out result))throw new InvalidOperationException("Original Feature not defined: "+name);
            return result;
        }
        private static string Attribute(XmlNode node,string name,string fallback="")
        {
            return node==null||node.Attributes[name]==null?fallback:node.Attributes[name].Value;
        }
        private static int Number(XmlNode node,int fallback=0)
        {
            return int.Parse(Attribute(node,"value",fallback.ToString(CultureInfo.InvariantCulture)),CultureInfo.InvariantCulture);
        }
        public OriginalFeatureGraph(string originalXml)
        {
            var document=new XmlDocument();document.XmlResolver=null;document.LoadXml(originalXml);
            foreach(XmlNode node in document.SelectNodes("/FEATURES/FeatureObj"))
            {
                var feature=new Feature{Name=Attribute(node.SelectSingleNode("FeatureName"),"name")};
                if(Number(node.SelectSingleNode("Feature[@type='Dynamic']"))!=0)
                    throw new InvalidOperationException("RCG graph contains an unsupported dynamic position");
                feature.Sequential=Number(node.SelectSingleNode("Feature[@type='PlayInSequence']"))!=0;
                feature.TransitionMilliseconds=Number(node.SelectSingleNode("FeatureTransDelay"));
                feature.StartX=checked((short)Number(node.SelectSingleNode("FeatureTransPos[@mode='start'][@type='x']")));
                feature.StartY=checked((short)Number(node.SelectSingleNode("FeatureTransPos[@mode='start'][@type='y']")));
                feature.EndX=checked((short)Number(node.SelectSingleNode("FeatureTransPos[@mode='end'][@type='x']")));
                feature.EndY=checked((short)Number(node.SelectSingleNode("FeatureTransPos[@mode='end'][@type='y']")));
                foreach(XmlNode child in node.ChildNodes)
                {
                    string prefix;
                    if(child.Name=="BitmapFeatureObj")prefix="Bitmap";
                    else if(child.Name=="AnimFeatureObj")prefix="Anim";
                    else if(child.Name=="AudioFeatureObj")prefix="Audio";
                    else if(child.Name=="MeterFeatureObj")prefix="Meter";
                    else if(child.Name=="TriggerFeatureObj")
                    {
                        feature.Triggers.Add(new Trigger{Name=Attribute(child.SelectSingleNode("TriggerName"),"name"),
                            KillName=Attribute(child.SelectSingleNode("TriggerKillName"),"name"),Index=Number(child.SelectSingleNode("TriggerIndex")),
                            WinDataOffset=Number(child.SelectSingleNode("TriggerWinDataOffset"),-1),
                            WaitForCompletion=Number(child.SelectSingleNode("TriggerDone"))==1});
                        continue;
                    }
                    else continue;
                    var atom=new Atom{Type=prefix,Name=Attribute(child.SelectSingleNode(prefix+"Name"),"name")};
                    foreach(XmlNode property in child.ChildNodes)
                        if(property.NodeType==XmlNodeType.Element&&property.Name!=prefix+"Name")
                            atom.Properties.Add(property.Name,Attribute(property,"value"));
                    feature.Objects.Add(atom);
                }
                if(feature.Objects.Count!=Number(node.SelectSingleNode("FeatureObjCount"))||
                    feature.Triggers.Count!=Number(node.SelectSingleNode("FeatureTriggerCount")))
                    throw new InvalidOperationException("Original Feature object counts disagree: "+feature.Name);
                ids.Add(feature.Name,features.Count);features.Add(feature);
            }
            foreach(Feature feature in features)foreach(Trigger trigger in feature.Triggers)
            {
                if(trigger.Name.Length!=0)Id(trigger.Name);
                if(trigger.KillName.Length!=0)Id(trigger.KillName);
            }
        }
    }
}
