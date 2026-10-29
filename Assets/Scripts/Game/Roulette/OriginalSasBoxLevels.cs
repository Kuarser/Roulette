using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;

namespace RouletteDeluxe
{
    // Only the native level/count lookup is recovered here. This is not a
    // JackpotBox media renderer or a general replacement for CFGHybridDictionary.
    public sealed class OriginalSasBoxLevels
    {
        public const string PackagedDefinitionSha256 = "436F162D9D0A2312570227C96C7A634127F9FEA5939597673721D7F1F5D8F1F8";
        private readonly int[,] bounds = new int[5,3];
        private readonly int[] counts = new int[5];
        public string DefinitionSha256 { get; private set; }

        private OriginalSasBoxLevels()
        {
            // FileDefs::JackpotBoxLevelStruct constructor initializes -1.
            // LoadJackpotBox retains physical slots; it does not compact holes.
            for(int type=0;type<5;type++)for(int level=0;level<3;level++)bounds[type,level]=-1;
        }
        public static OriginalSasBoxLevels ReadSnapshot(byte[] xml)
        {
            if(xml==null)throw new ArgumentNullException("xml");
            var result=new OriginalSasBoxLevels();
            using(var hash=SHA256.Create())result.DefinitionSha256=BitConverter.ToString(hash.ComputeHash(xml)).Replace("-","");
            var found=new bool[5,3];
            using(var stream=new MemoryStream(xml,false))
            using(var reader=XmlReader.Create(stream,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null}))
            {
                while(reader.Read())
                {
                    if(reader.NodeType!=XmlNodeType.Element)continue;
                    string name=reader.GetAttribute("name");
                    if(name==null)continue;
                    for(int type=0;type<5;type++)for(int level=0;level<3;level++)
                    {
                        if(name!="JackpotBoxType"+(type+1)+"Level"+(level+1)+"CreditUpperBound")continue;
                        int bound;
                        if(reader.LocalName!="int32" || found[type,level] ||
                            !int.TryParse(reader.GetAttribute("value"),NumberStyles.Integer,CultureInfo.InvariantCulture,out bound))
                            throw new PcRoundLifecycleDataException("SAS box upper bound is duplicated or is not int32: "+name);
                        found[type,level]=true;result.bounds[type,level]=bound<0?int.MaxValue:bound;result.counts[type]++;
                    }
                }
            }
            return result;
        }
        public static OriginalSasBoxLevels LoadPackagedSnapshot()
        {
            string path=Path.Combine(Application.streamingAssetsPath,"Original/definitions/BAL_246490A/RouletteDeluxeSZ/filedefs.xml");
            var result=ReadSnapshot(File.ReadAllBytes(path));
            // No additional deployed definition/override has been admitted.
            if(result.DefinitionSha256!=PackagedDefinitionSha256)
                throw new PcRoundLifecycleDataException("SAS packaged filedefs differs from the admitted original snapshot");
            return result;
        }
        private static void RequireType(int type)
        {if(type<0||type>=5)throw new PcRoundLifecycleDataException("Native JackpotBox type is outside 0..4");}
        public int LevelCount(int internalType){RequireType(internalType);return counts[internalType];}
        public int LevelUpperBound(int internalType,int level)
        {
            RequireType(internalType);
            if(level<0||level>=counts[internalType])
                throw new PcRoundLifecycleDataException("Native JackpotBox XML Type"+(internalType+1)+" Level"+(level+1)+" is missing (level < JackpotBoxLevelCounts[type])");
            return bounds[internalType,level];
        }
        public int GetLevel(int internalType,long amount)
        {
            RequireType(internalType);
            for(int level=0;level<counts[internalType];level++)if(amount<=LevelUpperBound(internalType,level))return level;
            return -1;
        }
        public OriginalSasBoxSelection SelectMultiplierDisplay(bool sharedBoxShown)
        {
            // Game::ShowMultiplierBonus chooses internal type2 with money0,
            // then ShowJackpotBox displays internal type3 with that same level.
            int level=GetLevel(2,0);
            if(level>2)throw new PcRoundLifecycleDataException("Native SAS box level exceeds 2");
            // The native shared-window test precedes LevelInfo. This boolean
            // describes a live window, never the persisted display.shown byte.
            if(sharedBoxShown)return new OriginalSasBoxSelection(level,true);
            if(level>=0)LevelUpperBound(3,level);
            return new OriginalSasBoxSelection(level,false);
        }
    }
    public sealed class OriginalSasBoxSelection
    {
        public readonly int Level;
        public readonly bool SharedWindowAlreadyShown;
        public bool UsesNativeDefaultPath { get {return Level<0&&!SharedWindowAlreadyShown;} }
        internal OriginalSasBoxSelection(int level,bool shared){Level=level;SharedWindowAlreadyShown=shared;}
    }
}
