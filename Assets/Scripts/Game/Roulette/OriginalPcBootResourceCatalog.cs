using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
namespace RouletteDeluxe
{
 // Immutable packaged inputs and a separate caller-owned PC NV directory.
 // A seed entry supplies the simulated source; it never initializes persistent NV.
 public sealed class OriginalPcBootResourceCatalog
 {
  [Serializable] private sealed class Entry{public string bundlePath,virtualPath,sha256;public bool seedOnly;}
  [Serializable] private sealed class Manifest{public Entry[] entries;}
  private readonly string root,nv;private readonly Dictionary<string,string> paths=new Dictionary<string,string>(StringComparer.Ordinal);private readonly HashSet<string> verified=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  public string BindingRoot{get{return Path.Combine(root,"bindings");}}
  public OriginalPcBootResourceCatalog(string bootRoot,string nvDirectory)
  {
   if(string.IsNullOrEmpty(bootRoot)||string.IsNullOrEmpty(nvDirectory))throw new ArgumentException("Packaged boot and separate PC NV paths required");root=Path.GetFullPath(bootRoot);nv=Path.GetFullPath(nvDirectory);
   string packagedPrefix=root.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)+Path.DirectorySeparatorChar;if(string.Equals(root.TrimEnd(Path.DirectorySeparatorChar),nv.TrimEnd(Path.DirectorySeparatorChar),StringComparison.OrdinalIgnoreCase)||nv.StartsWith(packagedPrefix,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("PC NV storage must be outside packaged boot resources");
   string manifestPath=Path.Combine(root,"resources_manifest_v4.json");if(!File.Exists(manifestPath))manifestPath=Path.Combine(root,"resources_manifest_v3.json");var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));if(manifest==null||manifest.entries==null)throw new InvalidDataException("Original boot resource manifest missing");
   foreach(var entry in manifest.entries)
   {
    string file=Contained(entry.bundlePath);using(var algorithm=SHA256.Create()){string hash=BitConverter.ToString(algorithm.ComputeHash(File.ReadAllBytes(file))).Replace("-","");if(!string.Equals(hash,entry.sha256,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Original boot resource hash mismatch: "+entry.bundlePath);}
    if(!verified.Add(file))throw new InvalidDataException("Duplicate packaged boot resource: "+entry.bundlePath);if(!string.IsNullOrEmpty(entry.virtualPath))paths.Add(entry.virtualPath,file);
   }
  }
  private string Contained(string relative)
  {
   if(string.IsNullOrEmpty(relative)||Path.IsPathRooted(relative))throw new InvalidDataException("Relative boot resource path required");string file=Path.GetFullPath(Path.Combine(root,relative));if(!file.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Boot resource leaves packaged directory");return file;
  }
  public string Resolve(string originalPath)
  {
   if(originalPath=="/nvram_os/jurisdiction.bin")return Path.Combine(nv,"jurisdiction.bin");string file;if(!paths.TryGetValue(originalPath,out file))throw new FileNotFoundException("Original PC boot input is not packaged",originalPath);return file;
  }
  public string Binding(string name){string file=Contained("bindings/"+name);if(!verified.Contains(file))throw new InvalidDataException("Binding is absent from verified boot manifest: "+name);return file;}
 }
}
