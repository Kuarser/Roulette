using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace RouletteDeluxe
{
    // Exact decoded Bink frame provider. Scheduling, repeat count and lifecycle remain caller-owned.
    // It retains one texture per layer instead of loading every movie frame into memory.
    public sealed class OriginalCashoutMovieFrames : IDisposable
    {
        [Serializable] public sealed class Frame { public string file,pngSha256,rgbaSha256; public int alphaMin,alphaMax; }
        [Serializable] public sealed class Clip
        {
            public string nativePath,folder;
            public int width,height,frameCount,sourceFrameRateNumerator,sourceFrameRateDenominator;
            public Frame[] frames;
        }
        [Serializable] public sealed class Alias { public string nativePath,targetNativePath; }
        [Serializable] public sealed class Manifest { public int schema,decodedFrameCount; public Clip[] clips; public Alias[] aliases; }
        public readonly Clip Source;
        public Texture2D Texture {get;private set;}
        public int FrameIndex {get;private set;}=-1;
        readonly string root;
        readonly Func<string,byte[]> read;
        bool disposed;
        public static Manifest LoadManifest()
        {
            var asset=Resources.Load<TextAsset>("Original/CashOutMovies/manifest");
            if(asset==null)throw new FileNotFoundException("CashOut decoded movie manifest missing");
            var manifest=JsonUtility.FromJson<Manifest>(asset.text);
            if(manifest==null||manifest.schema!=1||manifest.clips==null||manifest.aliases==null)throw new InvalidDataException("Invalid CashOut movie manifest");
            return manifest;
        }
        public OriginalCashoutMovieFrames(Manifest manifest,string nativePath,string streamingRoot,Func<string,byte[]> readFile=null)
        {
            if(manifest==null||nativePath==null||streamingRoot==null)throw new ArgumentNullException();
            int nul=nativePath.IndexOf('\0');if(nul>=0)nativePath=nativePath.Substring(0,nul);
            foreach(var alias in manifest.aliases)if(alias.nativePath==nativePath){nativePath=alias.targetNativePath;break;}
            foreach(var clip in manifest.clips)if(clip.nativePath==nativePath){Source=clip;break;}
            if(Source==null)throw new FileNotFoundException("No decoded CashOut Bink stream",nativePath);
            if(Source.frames==null||Source.frames.Length!=Source.frameCount||Source.width<=0||Source.height<=0)throw new InvalidDataException("Invalid CashOut clip");
            root=Path.Combine(streamingRoot,"Original/CashOutMovies",Source.folder);read=readFile??File.ReadAllBytes;
            Texture=new Texture2D(Source.width,Source.height,TextureFormat.RGBA32,false,false) {name=Source.folder,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
        }
        public void SetFrame(int index)
        {
            if(disposed)throw new ObjectDisposedException(nameof(OriginalCashoutMovieFrames));
            if(index<0||index>=Source.frameCount)throw new ArgumentOutOfRangeException(nameof(index));
            if(index==FrameIndex)return;
            byte[] png=read(Path.Combine(root,Source.frames[index].file));
            if(!Texture.LoadImage(png,false)||Texture.width!=Source.width||Texture.height!=Source.height)
                throw new InvalidDataException("Decoded CashOut frame dimensions differ");
            Texture.filterMode=FilterMode.Bilinear;Texture.wrapMode=TextureWrapMode.Clamp;
            FrameIndex=index;
        }
        public void Dispose()
        {
            if(disposed)return;disposed=true;
            if(Application.isPlaying)UnityEngine.Object.Destroy(Texture);else UnityEngine.Object.DestroyImmediate(Texture);
            Texture=null;
        }
    }
    public static class OriginalCashoutMovieFramesValidation
    {
        static string Hash(byte[] bytes)
        {using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-","");}
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut decoded texture regression "+checks);++checks;};
            var manifest=OriginalCashoutMovieFrames.LoadManifest();check(manifest.clips.Length==10&&manifest.decodedFrameCount==690);
            int frameCount=0;
            foreach(var clip in manifest.clips)
            {
                int reads=0;
                using(var movie=new OriginalCashoutMovieFrames(manifest,clip.nativePath,Application.streamingAssetsPath,p=>{++reads;return File.ReadAllBytes(p);}))
                {
                    var texture=movie.Texture;check(movie.FrameIndex==-1);
                    byte[] rgba=new byte[clip.width*clip.height*4];
                    for(int index=0;index<clip.frameCount;++index)
                    {
                        movie.SetFrame(index);check(movie.Texture==texture&&movie.FrameIndex==index&&texture.width==clip.width&&texture.height==clip.height);
                        // Unity pixels begin at bottom-left; PNG/rawvideo reference begins at top-left.
                        var pixels=texture.GetPixels32();int lo=255,hi=0;
                        for(int y=0;y<clip.height;++y)for(int x=0;x<clip.width;++x)
                        {
                            var c=pixels[(clip.height-1-y)*clip.width+x];int target=(y*clip.width+x)*4;
                            rgba[target]=c.r;rgba[target+1]=c.g;rgba[target+2]=c.b;rgba[target+3]=c.a;
                            lo=Math.Min(lo,c.a);hi=Math.Max(hi,c.a);
                        }
                        check(Hash(rgba)==clip.frames[index].rgbaSha256);check(lo==clip.frames[index].alphaMin&&hi==clip.frames[index].alphaMax);
                        movie.SetFrame(index);check(reads==index+1);++frameCount;
                    }
                    bool rejected=false;try{movie.SetFrame(clip.frameCount);}catch(ArgumentOutOfRangeException){rejected=true;}check(rejected&&reads==clip.frameCount);
                }
            }
            check(frameCount==690);
            foreach(var alias in manifest.aliases)
            {
                using(var movie=new OriginalCashoutMovieFrames(manifest,alias.nativePath+"\0ignored",Application.streamingAssetsPath))
                {check(movie.Source.nativePath==alias.targetNativePath);movie.SetFrame(0);check(movie.FrameIndex==0);}
            }
            var disposed=new OriginalCashoutMovieFrames(manifest,manifest.clips[0].nativePath,Application.streamingAssetsPath);
            disposed.Dispose();disposed.Dispose();bool closed=false;try{disposed.SetFrame(0);}catch(ObjectDisposedException){closed=true;}check(closed&&disposed.Texture==null);
            UnityEngine.Debug.Log("CASHOUT_MOVIE_TEXTURES_VALIDATED: clips=10 frames="+frameCount+" aliases="+manifest.aliases.Length+" checks="+checks);
            return checks;
        }
    }
}
